import {
    AfterViewInit, Component, HostListener, OnDestroy,
    ViewChild
} from "@angular/core";
import {MobilePhoneService} from "../../../../core/services/phone-models/phone-model.service";
import {ColorScheme} from "../../../../core/models/blank/color-scheme";
import {ColorSchemeService} from "../../../../core/services/color-scheme/color-scheme.service";
import {BlankService} from "../../../../core/services/blank/blank.service";
import {Blank} from "../../../../core/models/blank/blank";
import {BlankFilter} from "../../../../core/models/blank/blank-filter";
import {PagingOptions, PagingState} from "../../../../core/models/base/paging-options.model";
import {ConstructorModelComponent} from "./constructor-model/constructor-model.component";
import * as _ from 'lodash';
import {GlobalLoadingIndicatorService} from "../../services/ui-services/global-loading-indicator.service";
import {BlankBatchService} from "../../../../core/services/blank/blank-batch.service";
import {BlankBatch} from "../../../../core/models/blank/blank-batch";
import {ConstructorHeaderComponent} from "./constructor-header/constructor-header.component";
import {NavbarManagmentService, NavbarState} from "../../services/ui-services/navbar-managment.service";
import {ConstructorModelParams} from "./constructor-model/models/constructor-params";
import {LocalStorageService} from "../../../../core/services/base/local-storage.service";
import {NavigationStart, Router} from "@angular/router";
import {ConstructorHeaderViewModel} from "./constructor-header/constructor-header.viewmodel";
import {SettingsProvider} from "../../services/settings-provider/settings-provider";
import {ConstructorPagingComponent} from "./constructor-paging/constructor-paging.component";
import {Consts} from "../../../../core/data/consts";
import {DataTransferProvider} from "../../../../core/services/base/data-transfer.provider";
import {DataTransferKeys} from "../../common/data-transfer-keys";
import {ConstructorModelMode} from "./constructor-model/models/constructor-model-mode";
import {CartItem, CartService} from "../../services/cart-service/cart.service";
import {PhoneModel} from "../../../../core/models/phone-model/phone-model.model";
import {ToastrService} from "ngx-toastr";
import "rxjs-compat/add/operator/filter";
import {Subscription} from "rxjs/Rx";
import {OrderService} from "../../../../core/services/order/order-service";
import {ScreenHelper} from "../../../../core/helpers/screen-helper";
import {ScrollToService, ScrollToConfigOptions } from '@nicky-lenaers/ngx-scroll-to';
import {ConstructorPhoneSelectorMobileComponent} from "./mobile/constructor-phone-selector.mobile/constructor-phone-selector.mobile.component";
import {ConstructorBlankSelectorMobileComponent} from "./mobile/constructor-blank-selector.mobile/constructor-blank-selector.mobile.component";

@Component({
    moduleId: module.id.toString(),
    templateUrl: 'constructor-page.component.html',
    styles: [
        require('./constructor-page.component.scss').toString(),
        require('./constructor-page.mobile.component.scss').toString()
    ]
})
export class ConstructorPageComponent implements AfterViewInit, OnDestroy {
    private readonly ConstructorState_LSKey = 'ConstructorState';

    public ConstructorModeEnum = ConstructorMode;

    public products: Array<Blank> = [];
    public phoneModels: Array<PhoneModel> = [];
    public colorSchemes: Array<ColorScheme> = [];
    public blankBatches: Array<BlankBatch> = [];

    public blankFilter: BlankFilter = new BlankFilter();
    public constructorModelParams: ConstructorModelParams = new ConstructorModelParams();

    public pagingState: PagingState;

    private constructorHeaderVM: ConstructorHeaderViewModel;

    private modeOfConstructorModel = ConstructorModelMode.Full;

    private isMobile = ScreenHelper.IsMobile();

    //region Inner Elements

    @ViewChild(ConstructorModelComponent)
    private constructorModelComponent: ConstructorModelComponent;

    /*private constructorModelComponent: ConstructorModelComponent;

    @ViewChild(ConstructorModelComponent)
    private set constructorModelComponentResolver(component: ConstructorModelComponent) {
        this.constructorModelComponent = component;
        this.restoreConstructorModelViewModelIfExists();
    }*/

    @ViewChild(ConstructorHeaderComponent)
    private constructorHeaderComponent: ConstructorHeaderComponent;

    @ViewChild(ConstructorPagingComponent)
    private pagingComponent: ConstructorPagingComponent;

    @ViewChild(ConstructorPhoneSelectorMobileComponent)
    private constructorPhoneSelectorMobileComponent: ConstructorPhoneSelectorMobileComponent;

    @ViewChild(ConstructorBlankSelectorMobileComponent)
    private constructorBlankSelectorMobileComponent: ConstructorBlankSelectorMobileComponent;

    //endregion

    public constructor(
        private cartService: CartService,
        private productService: BlankService,
        private colorSchemeService: ColorSchemeService,
        private phoneModelService: MobilePhoneService,
        private blankBatchService: BlankBatchService,
        private loadingIndicatorService: GlobalLoadingIndicatorService,
        private navbarManagmentService: NavbarManagmentService,
        private localStorageService: LocalStorageService,
        private router: Router,
        private dataTransferProvider: DataTransferProvider,
        private notificationService: ToastrService,
        private orderService: OrderService,
        private scrollToService: ScrollToService
    ) {
        // Navbar should be collapsed:
        this.navbarManagmentService.StateChange(NavbarState.Collapsed);
    }

    //region Subscriptions

    private onCartClearedSubscription: Subscription;
    private onCartItemRemovedSubscription: Subscription;
    private navigationStartEventSubscription: Subscription;

    //endregion

    ngOnDestroy(): void {
        this.onCartClearedSubscription.unsubscribe();
        this.onCartItemRemovedSubscription.unsubscribe();

        this.navigationStartEventSubscription.unsubscribe();
    }

    public async ngAfterViewInit() {
        this.loadingIndicatorService.Loading();

        await Promise.all([this.readColorSchemes(), this.readMobilePhones()]);

        this.setDefaults();

        await this.readBlankBatches();

        this.blankFilter.BatchId = Consts.ItemForAllId;

        let editingFromCartMode = this.dataTransferProvider.Exists(DataTransferKeys.ConstructorPage_InitializeFromCartForCaseEditing);

        if (SettingsProvider.IsConstructorSavingStateEnabled() && editingFromCartMode === false) {
            try {
                this.restorePreviousStateIfExists();
                this.restoreConstructorModelViewModelIfExists();
            } catch {
                this.localStorageService.ClearState(this.ConstructorState_LSKey);
                this.setDefaults();
                this.blankFilter.BatchId = Consts.ItemForAllId;
            }
        }

        await this.readByFilter();

        this.constructorModelParams.Blank = this.products[0];
        this.constructorModelParams.PhoneModel =
            _.find(this.phoneModels, (p: PhoneModel) => p.Id === this.blankFilter.PhoneModelId);

        this.constructorModelParams.PushPhoneModelColoredCase();

        this.constructorHeaderVM = new ConstructorHeaderViewModel(this.blankBatches, this.colorSchemes, this.phoneModels);
        this.constructorHeaderVM.AddFilter(this.blankFilter);

        this.initializeWithExternalParametersIfNeed();

        this.subscribeEvents();

        this.loadingIndicatorService.Ready();
    }

    //region Cart Events Handlers

    private onCartCleared_Handler = async () => {
        await this.readByFilter(true)
    }

    private onCartItemRemoved_Handler = async () => {
        await this.readByFilter(true);
    }

    //endregion

    //region Save/Restore State Tab/Browser Close

    private subscribeEvents = () => {

        // It need to save state, if route has been changed:
        this.registerRouteChangeCallback();

        this.onCartItemRemovedSubscription =
            this.cartService.OnItemRemoved.subscribe(async ($event: any) => this.onCartItemRemoved_Handler());

        this.onCartClearedSubscription =
            this.cartService.OnCartCleared.subscribe(async ($event: any) => await this.onCartCleared_Handler());
    }

    private registerRouteChangeCallback= () => {
        this.navigationStartEventSubscription = this.router.events
            .filter(event => event instanceof NavigationStart)
            .subscribe((event:NavigationStart) => this.saveState());
    }

    @HostListener('window:beforeunload', ['$event'])
    private unloadNotification = ($event: any) => {
        this.saveState();
    }

    private restorePreviousStateIfExists = () => {
        let previousState = this.localStorageService.RestoreState<ConstructorState>(this.ConstructorState_LSKey);
        if (previousState !== null) {

            // Test state:
            previousState.PagingState.Size;

            // Update paging state:
            this.pagingState = previousState.PagingState;

            // Update filter:
            this.blankFilter.BatchId = previousState.BatchId;
            this.blankFilter.ColorSchemeId = previousState.ColorSchemeId;
            this.blankFilter.PhoneModelId = previousState.PhoneModelId;
            this.blankFilter.PhoneModelColoredBlankId = previousState.PhoneModelColoredBlankId;
        }
    }

    private restoreConstructorModelViewModelIfExists() {
        let previousState = this.localStorageService.RestoreState<ConstructorState>(this.ConstructorState_LSKey);
        if (previousState !== null) {

            // Update constructor model:
            this.constructorModelComponent.ViewModel = previousState.Model;
        }
    }

    private saveState = () => {
        if (SettingsProvider.IsConstructorSavingStateEnabled() === false) {
            return;
        }

        // It doesn't need to save state if editing occurs from cart:
        if (this.isEditingFromCart === true) {
            return;
        }

        let state = <ConstructorState> {
            BatchId: this.blankFilter.BatchId,
            ColorSchemeId: this.blankFilter.ColorSchemeId,
            Model: this.constructorModelComponent.ViewModel,
            PagingState: this.pagingState,
            PhoneModelId: this.blankFilter.PhoneModelId,
            PhoneModelColoredBlankId: this.blankFilter.PhoneModelColoredBlankId
        };

        this.localStorageService.SaveState(this.ConstructorState_LSKey, state);
    }

    //endregion

    //region ProductFilter Change Events

    private onBlankBatchChange = async (blankBatchId: number) => {
        this.blankFilter.BatchId = blankBatchId;

        await this.readByFilter();
    }

    private onColorSchemeChange = async (colorSchemeId: number) => {
        this.blankFilter.BatchId = null;
        this.blankFilter.ColorSchemeId = colorSchemeId;

        await this.readByFilter();
        await this.readBlankBatches();

        // TODO: make more common implementation:
        this.blankFilter.BatchId = Consts.ItemForAllId;

        if(ScreenHelper.IsMobile() === true) {
            this.constructorBlankSelectorMobileComponent.SelectColorScheme(this.blankFilter.ColorSchemeId);

            this.constructorBlankSelectorMobileComponent.UpdateBlankBatches(this.blankBatches);
            this.constructorBlankSelectorMobileComponent.SelectBlankBatch(this.blankFilter.BatchId);
        } else {
            this.constructorHeaderComponent.SelectColorScheme(this.blankFilter.ColorSchemeId);

            this.constructorHeaderComponent.UpdateBlankBatches(this.blankBatches);
            this.constructorHeaderComponent.SelectBlankBatch(this.blankFilter.BatchId);
        }
    }

    private onPhoneModelChange = (phoneModelId: number) => {
        this.blankFilter.PhoneModelId = phoneModelId;

        var phoneModel = _.find(this.phoneModels, (phoneModel: PhoneModel) => phoneModel.Id === this.blankFilter.PhoneModelId);

        this.constructorModelComponent.UpdatePhoneModel(phoneModel);

        if(ScreenHelper.IsMobile() === true) {
            this.constructorPhoneSelectorMobileComponent.UpdateColors(phoneModel.AvailableColors);
        } else {
            this.constructorHeaderComponent.UpdateColors(phoneModel.AvailableColors);
        }
    }

    private onColorChange = (coloredBlankId: number) => {
        this.blankFilter.PhoneModelColoredBlankId = coloredBlankId;

        this.constructorModelComponent.UpdatePhoneModelCaseColor(coloredBlankId);
    }

    private onPagingStateChange = () => {
        this.readByFilter();
    }

    //endregion



    private setDefaults() {
        this.blankFilter.PhoneModelId = this.phoneModels[2].Id;
        this.blankFilter.ColorSchemeId = Consts.ItemForAllId;

        if(this.phoneModels[0].AvailableColors.length > 0) {
            this.blankFilter.PhoneModelColoredBlankId = this.phoneModels[0].AvailableColors[0].Id;
        }

        this.pagingState = new PagingState();

        this.pagingState.Size = SettingsProvider.GetConstructorPageSize();
        this.pagingState.CurrentNumber = 1;
        this.pagingState.AllPagesCount = 1;
    }

    //region Reading Data

    private async readMobilePhones() {
        let phoneModelReadingResult = await this.phoneModelService.ReadAll();
        if (phoneModelReadingResult.Ok) {
            this.phoneModels = phoneModelReadingResult.Result;
        }
    }

    private async readColorSchemes() {
        var readingResult = await this.colorSchemeService.Read();
        if (readingResult.Ok) {
            this.colorSchemes = readingResult.Result;
        }
    }

    private async readBlankBatches() {
        var readingResult = await this.blankBatchService.ReadByColorSchemeId(this.blankFilter.ColorSchemeId);
        if (readingResult.Ok) {
            this.blankBatches = readingResult.Result;
        }
    }

    public async readByFilter(withoutDataUpdate: boolean = false) {
        await this.readBlankBatches();

        this.blankFilter.PagingOptions = PagingOptions.Create(this.pagingState.Size, this.pagingState.CurrentNumber);

        if (ScreenHelper.IsMobile() === true) {
            this.blankFilter.PagingOptions = null;
        }

        this.applyCartToBlankFilter();

        var readingResult = await this.productService.ReadPagedByFilter(this.blankFilter);
        if (readingResult.Result) {
            this.pagingState.CurrentNumber = readingResult.Result.PageNumber;
            this.pagingState.AllPagesCount = readingResult.Result.AllPagesCount;
            this.pagingState.AllElementCount = readingResult.Result.AllElementCount;

            if (ScreenHelper.IsMobile() !== true) {
                this.pagingComponent.UpdateFrame();
            }

            this.products = readingResult.Result.Data;

            if(withoutDataUpdate === false) {
                this.constructorModelParams.Blank = this.products[0];
            }
        }
    }

    private applyCartToBlankFilter = () => {
        this.blankFilter.IgnoredBlankIds = this.cartService.GetInstance().Items.map(item => item.CaseParameters.Blank.Id);
    }

    private onAddToCart = async () => {
        await this.readByFilter(true);
        this.constructorModelComponent.UpdateBlank(this.products[0]);

        this.notificationService.success('Чехол добавлен', null, {
            timeOut: 2000,
            positionClass: 'toast-top-center'
        })

    }

    //endregion

    public updateConstructor(blank: Blank) {
        this.constructorModelComponent.UpdateBlank(blank);

        if (ScreenHelper.IsMobile() === true) {
            this.scrollToModel();
        }
    }

    private scrollToModel(offset: number = 0) {
        const config: ScrollToConfigOptions = {
            target: 'modelContainer',
            offset: offset
        };

        this.scrollToService.scrollTo(config)
    }

    //region Initialize With External Parameters

    private isEditingFromCart: boolean = false;

    private initializeWithExternalParametersIfNeed = () => {
        let initFromCartParams =
            this.dataTransferProvider.Get<ConstructorPage_InitializeFromCartForCaseEditing>(DataTransferKeys.ConstructorPage_InitializeFromCartForCaseEditing);

        if (initFromCartParams) {
            this.isEditingFromCart = true;

            this.constructorModelComponent.SetCartItemForEditing(initFromCartParams.CartItem);

            this.constructorModelParams = initFromCartParams.CartItem.CaseParameters.Clone();
            this.modeOfConstructorModel = ConstructorModelMode.FullForEditingFromCart;

            this.dataTransferProvider.Delete(DataTransferKeys.ConstructorPage_InitializeFromCartForCaseEditing);

            if (ScreenHelper.IsMobile() === true) {
                this.scrollToModel(150);
            }
        }
    }

    //endregion
}

export class ConstructorState {
    BatchId: number;
    ColorSchemeId: number;
    Model: ConstructorModelParams;
    PagingState: PagingState;
    PhoneModelId: number;
    PhoneModelColoredBlankId: number;
}

export class ConstructorPage_InitializeFromCartForCaseEditing {

    constructor(item: CartItem) {
        this.CartItem = item;
    }

    CartItem: CartItem;
}

export enum ConstructorMode {
    Main = 0,
    EditingFromCart = 1
}