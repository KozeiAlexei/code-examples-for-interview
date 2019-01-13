///<reference path="../../../../../../node_modules/@angular/core/src/metadata/lifecycle_hooks.d.ts"/>
import {Component, ElementRef, EventEmitter, HostListener, Input, OnDestroy, Output, ViewChild} from "@angular/core";
import {Blank} from "../../../../../core/models/blank/blank";
import {CartItem, CartService} from "../../../services/cart-service/cart.service";
import {ScreenHelper} from "../../../../../core/helpers/screen-helper";
import {MathHelper} from "../../../../../core/helpers/math-helper";
import {Vector2D} from "../../../../../core/models/math/vector-2d";
import {ConstructorModelParams} from "./models/constructor-params";
import {ConstructorModelMode} from "./models/constructor-model-mode";
import {CaseRenderingService} from "../../../../../core/services/business/case-rendering/case-rendering.service";
import {Point, RenderRequest, TemplateSize} from "../../../../../core/models/case-rendering/render-request";
import {Router} from "@angular/router";
import {isNullOrUndefined} from "util";
import {PhoneModel} from "../../../../../core/models/phone-model/phone-model.model";
import {Observable, Subscription} from "rxjs/Rx";
import {WPH} from "../../../../../core/helpers/wp-helper";

@Component({
    selector: 'constructor-model',
    moduleId: WPH.ModuleSelector(module),
    templateUrl: 'constructor-model.component.html',
    styles: [
        require('./constructor-model.component.scss').toString(),
        require('./constructor-model.mobile.component.scss').toString()
    ]
})
export class ConstructorModelComponent implements OnDestroy{

    //region Constructor
    constructor(
        private router: Router,
        private cartService: CartService,
        private caseRenderingService: CaseRenderingService) {

        this.dpi = ScreenHelper.GetDPI();

        this.loadingModel = new LoadingModel();
        this.loadingSubcription = this.loadingObservable.subscribe(this.loadingObserver);
    }

    //endregion Constructor

    //region Startup

    private loadingModel: LoadingModel;
    private loadingObservable = Observable.of<ConstructoModelLoadingCode>();
    private loadingSubcription: Subscription;

    private loadingObserver = {
        next: (code: ConstructoModelLoadingCode) => {
            switch (code) {
                case ConstructoModelLoadingCode.Blank:
                    this.loadingModel.IsBlankLoaded = true;
                    break;
                case ConstructoModelLoadingCode.PhoneModel:
                    this.loadingModel.IsPhoneModelLoaded = true;
                    break;
                case ConstructoModelLoadingCode.PhoneModelWithBackground:
                    this.loadingModel.IsPhoneModelWithBackgroundLoaded = true;
                    break;
            }

            if (this.loadingModel.AreAllLoaded) {
                this.applySizesForTemplate();
                this.applySizesToTemplateWithBackground();

                this.isReady = true;
                this.loadingModel = new LoadingModel();
            }
        }
    }

    //endregion Startup

    //region Attributes

    @Input()
    constructorParams: ConstructorModelParams;

    @Input()
    mode: ConstructorModelMode = ConstructorModelMode.Full;

    @Output()
    onAddToCart: EventEmitter<any> = new EventEmitter(true);

    //endregion

    //region Actions

    UpdateBlank = (blank: Blank) => {
        this.constructorParams.Blank = blank;
        this.applySizesForTemplate();
    }

    UpdatePhoneModel = (phoneModel: PhoneModel) => {
        this.constructorParams.PhoneModel = phoneModel;
        this.constructorParams.Clear();

        this.constructorParams.PushPhoneModelColoredCase();

        this.applySizesForTemplate();
        this.applySizesToTemplateWithBackground();
    }

    UpdatePhoneModelCaseColor = (phoneModelCaseColoredBlankId: number) => {
        this.constructorParams.PhoneModelColoredBlankId = phoneModelCaseColoredBlankId;
        this.constructorParams.PushPhoneModelColoredCase();
    }

    RotateBlank = () => {
        this.constructorParams.IsBlankRotated = !this.constructorParams.IsBlankRotated;
    }

    ShowOnlyCase = () => {
        this.applySizesToTemplateWithBackground();
        this.showOnlyCase = !this.showOnlyCase;
    }

    SetCartItemForEditing = (cartItem: CartItem) => {
        this.cartItem = cartItem;
    }

    get ViewModel() {
        return this.constructorParams;
    }

    set ViewModel(model: ConstructorModelParams) {
        this.constructorParams = ConstructorModelParams.Restore(model);
        this.areConstructorParamsRestored = true;
    }

    //endregion

    //region Common UI Logic

    private get isComponentReady(): boolean {
        return (
            !isNullOrUndefined(this.constructorParams.PhoneModelColoredBlank) &&
            !isNullOrUndefined(this.constructorParams.PhoneModel) &&
            !isNullOrUndefined(this.constructorParams.Blank)
        );
    }

    private get areToolButtonsGroupVisible(): boolean {
        return (
            this.mode === ConstructorModelMode.Full ||
            this.mode === ConstructorModelMode.FullForEditingFromCart
        );
    }

    private get isFullMode(): boolean {
        return this.mode === ConstructorModelMode.Full;
    }

    private get isEditingFromCartMode(): boolean {
        return this.mode === ConstructorModelMode.FullForEditingFromCart;
    }

    //endregion

    //region Editing From Cart

    private saveChanges = async () => {
        this.applySizesToTemplateWithBackground();

        var renderingResult = await this.caseRenderingService.Render(this.buildRenderRequest());
        if (renderingResult.Ok) {
            this.cartItem.Blank = this.constructorParams.Blank;
            this.cartItem.PhoneModel = this.constructorParams.PhoneModel;
            this.cartItem.ColoredBlank = this.constructorParams.PhoneModelColoredBlank;
            this.cartItem.CaseParameters = this.constructorParams.Clone();
            this.cartItem.CaseImageURL = renderingResult.Result.DataUrl;
            this.cartService.UpdateCart();

            this.router.navigateByUrl('/cart');
        }
    }

    //endregion

    //region Enum Instances For View

    private ConstructorModelModeEnum = ConstructorModelMode;

    //endregion

    //region Inner Elements

    private blankTemplate: ElementRef;
    private phoneModelTemplate: ElementRef;
    private phoneModelWithBackgroundTemplate: ElementRef;

    @ViewChild('blankTemplate')
    private set blankTemplateContent(elementRef: ElementRef) {

        console.log('blank template VC')

        if(!elementRef) {
            return;
        }

        this.blankTemplate = elementRef;
        //this.loadingObserver.next(ConstructoModelLoadingCode.Blank);
    };

    @ViewChild('phoneModelTemplate')
    private set mobilePhoneTemplateContent(elementRef: ElementRef) {
        console.log('phone template VC')

        if(!elementRef) {
            return;
        }

        this.phoneModelTemplate = elementRef;
        //this.loadingObserver.next(ConstructoModelLoadingCode.PhoneModel);
    }

    @ViewChild('phoneModelTemplateWithBackground')
    private set mobilePhoneTemplateWithBackgroundContent(elementRef: ElementRef) {

        console.log('phone template with back VC')

        if(!elementRef) {
            return;
        }

        this.phoneModelWithBackgroundTemplate = elementRef;
        //this.loadingObserver.next(ConstructoModelLoadingCode.PhoneModelWithBackground);
    }

    private onBlankImageLoad(): void {

        if(this.isReady) {
            return;
        }

        console.log('blank loaded')

        this.loadingObserver.next(ConstructoModelLoadingCode.Blank);
    }

    private onPhoneModelImageLoad(): void {

        if(this.isReady) {
            return;
        }

        console.log('phobe loaded')

        this.loadingObserver.next(ConstructoModelLoadingCode.PhoneModel);
    }

    private onPhoneModelWithBackgroundImageLoad(): void {

        if(this.isReady) {
            return;
        }
        console.log('phone with back loaded')

        this.loadingObserver.next(ConstructoModelLoadingCode.PhoneModelWithBackground);
    }

    //endregion

    //region Private Members

    /** Depth per inch of user screen. */
    private dpi: number;

    private borderVector: BorderVector = null;
    private movingModel: MobilePhoneMovingModel = new MobilePhoneMovingModel();

    private showOnlyCase = false;

    private areConstructorParamsRestored = false;

    private cartItem: CartItem = null;

    private isReady = false;

    private addToCart = async () => {

        this.applySizesToTemplateWithBackground();

        var renderingResult = await this.caseRenderingService.Render(this.buildRenderRequest());
        if (renderingResult.Ok) {
            var item = new CartItem();

            item.Blank = this.constructorParams.Blank;
            item.PhoneModel = this.constructorParams.PhoneModel;
            item.ColoredBlank = this.constructorParams.PhoneModelColoredBlank;
            item.CaseParameters = this.constructorParams.Clone();
            item.CaseImageURL = renderingResult.Result.DataUrl;

            this.cartService.Add(item);

            this.onAddToCart.emit();
        }
    }

    private buildRenderRequest = (): RenderRequest => {
        return <RenderRequest> {
            IsBlankRotated: this.constructorParams.IsBlankRotated,
            BlankImageUrl: this.constructorParams.Blank.Image,
            BlankCaseImageUrl: this.constructorParams.PhoneModelColoredBlank.TemplateWithBackground.Path,
            BlankSize: <TemplateSize> {
                Width: this.blankTemplate.nativeElement.clientWidth,
                Height: this.blankTemplate.nativeElement.clientHeight,
                Offset: <Point> { X: 0, Y: 0 }
            },
            BlankCaseSize: <TemplateSize> {
                Width: this.constructorParams.Template.Width,
                Height: this.constructorParams.Template.Height,
                Offset: <Point> {
                    X: this.constructorParams.Template.Offset.Cx,
                    Y: this.constructorParams.Template.Offset.Cy
                }
            },
            BlankCaseWithBackgroundSize: <TemplateSize> {
                Width: this.constructorParams.TemplateWithBackground.Width,
                Height: this.constructorParams.TemplateWithBackground.Height,
                Offset: <Point> {
                    X: this.constructorParams.TemplateWithBackground.Offset.Cx,
                    Y: this.constructorParams.TemplateWithBackground.Offset.Cy
                }
            }
        }
    }

    //endregion

    //region OnDestroy Members

    ngOnDestroy(): void {
        this.loadingSubcription.unsubscribe();
    }

    //endregion

    //region Applying Sizes

    private applySizesForTemplate(isSizeChanged: boolean = false) {
        console.log('applying sized for template');

        this.calculateBlankSize();

        this.calculateTemplateSize();
        this.calculateBorders();

        if (this.constructorParams.Template.Offset === null || isSizeChanged) {
            this.calculateDefaultTemplateOffset();
        }

        this.updateBlankCss();
        this.updateCssSizeOfTemplate();
        this.updateCssOffsetOfTemplate();
    }

    private applySizesToTemplateWithBackground() {
        console.log('applying sized for template with background')

        if (this.constructorParams.Template.Offset !== null) {

            // Lazy initialization:
            if(this.constructorParams.TemplateWithBackground.Offset == null) {
                this.constructorParams.TemplateWithBackground.Offset = new Vector2D();
            }

            this.constructorParams.TemplateWithBackground.Width = Math.round(
                this.constructorParams.PhoneModelColoredBlank.TemplateWithBackground.Width / this.constructorParams.TemplateScaleVector.Cx);

            this.constructorParams.TemplateWithBackground.Height = Math.round(
                this.constructorParams.PhoneModelColoredBlank.TemplateWithBackground.Height / this.constructorParams.TemplateScaleVector.Cy);

            this.constructorParams.TemplateWithBackground.Offset.Cx = Math.round(
                this.constructorParams.Template.Offset.Cx -
                0.5 * (
                this.constructorParams.PhoneModelColoredBlank.TemplateWithBackground.Width -
                this.constructorParams.PhoneModelColoredBlank.Template.Width) / this.constructorParams.TemplateScaleVector.Cx);

            this.constructorParams.TemplateWithBackground.Offset.Cy = Math.round(
                this.constructorParams.Template.Offset.Cy -
                0.5 * (
                this.constructorParams.PhoneModelColoredBlank.TemplateWithBackground.Height -
                this.constructorParams.PhoneModelColoredBlank.Template.Height) / this.constructorParams.TemplateScaleVector.Cy);

            this.updateCssSizeOfTemplateWithBackground();
            this.updateCssOffsetOfTemplateWithBackground();
        }
    }

    //endregion

    //region Sizes/Offsets Calculation

    private calculateDefaultTemplateOffset = () => {

        var batchParams = this.constructorParams.Blank.Batch.BlankBatchParameters;

        this.constructorParams.Template.Offset = new Vector2D();

        this.constructorParams.Template.Offset.Cy = Math.round(
            this.borderVector.Top +
            (this.blankTemplate.nativeElement.clientHeight -
                MathHelper.Millimiters2Pixels(batchParams.OffsetTop, this.dpi) -
                MathHelper.Millimiters2Pixels(batchParams.OffsetBottom, this.dpi) - this.constructorParams.Template.Height) / 2);

        this.constructorParams.Template.Offset.Cx = Math.round(
            this.borderVector.Left +
            (this.blankTemplate.nativeElement.clientWidth -
                MathHelper.Millimiters2Pixels(batchParams.OffsetLeft, this.dpi) -
                MathHelper.Millimiters2Pixels(batchParams.OffsetRight, this.dpi) - this.constructorParams.Template.Width) / 2);

    }

    private calculateBlankSize() {
        let blank = this.constructorParams.Blank;
        let batchParameters = this.constructorParams.Blank.Batch.BlankBatchParameters;

        // By the ideal way, we should use sizes of each blank, but it is in future.
        this.constructorParams.BlankTemplate.Width = this.blankTemplate.nativeElement.clientWidth;
        this.constructorParams.BlankTemplate.Height =
            Math.round(this.blankTemplate.nativeElement.clientWidth * (batchParameters.Height / batchParameters.Width));
    }

    private calculateTemplateSize = () => {

        var blank = this.constructorParams.Blank;
        var blankParams = this.constructorParams.BlankTemplate;
        var mobilePhone = this.constructorParams.PhoneModel;
        var batchParams = this.constructorParams.Blank.Batch.BlankBatchParameters;

        // By the ideal way, we should use sizes of each blank, but it is in future.
        var scaleCoeff_W = blankParams.Width / MathHelper.Millimiters2Pixels(batchParams.Width, this.dpi);
        var scaleCoeff_H = blankParams.Height /  MathHelper.Millimiters2Pixels(batchParams.Height, this.dpi);

        this.constructorParams.Template.Width = Math.round(scaleCoeff_W * MathHelper.Millimiters2Pixels(mobilePhone.WidthMM, this.dpi));
        this.constructorParams.Template.Height = Math.round(scaleCoeff_H * MathHelper.Millimiters2Pixels(mobilePhone.HeightMM, this.dpi));

        if(this.constructorParams.Template.Height >= -1 && this.constructorParams.Template.Height <= 1) {
            alert('fail');
            debugger;
        }
    }

    private calculateBorders = () => {
        console.log('calc borders')

        var batchParams = this.constructorParams.Blank.Batch.BlankBatchParameters;

        this.borderVector = new BorderVector();

        this.borderVector.Top = Math.round(this.blankTemplate.nativeElement.offsetTop + MathHelper.Millimiters2Pixels(batchParams.OffsetTop, this.dpi));
        this.borderVector.Left = Math.round(this.blankTemplate.nativeElement.offsetLeft + MathHelper.Millimiters2Pixels(batchParams.OffsetLeft, this.dpi))

        // In the past we used nativeElement.offsetTop and nativeElement.OffsetLeft
        // for calculating, but now it isn't need.
        this.borderVector.Bottom = Math.round(this.constructorParams.BlankTemplate.Height -
            MathHelper.Millimiters2Pixels(batchParams.OffsetBottom, this.dpi) - this.constructorParams.Template.Height);

        this.borderVector.Right = Math.round(this.constructorParams.BlankTemplate.Width -
            MathHelper.Millimiters2Pixels(batchParams.OffsetRight, this.dpi) - this.constructorParams.Template.Width);
    }

    //endregion

    //region Template Properties

    private get templateRotationCss() : string {
        var angle = 0;
        if(this.constructorParams.IsBlankRotated) {
            angle = 180;
        }

        return `rotate(${angle}deg)`;
    }

    //endregion

    //region Template Moving Events

    private template_OnMouseDown = ($event: MouseEvent) => {

        console.log('start');

        if (this.mode === ConstructorModelMode.OnlyView) {
            return;
        }

        if($event.cancelable) {
            $event.preventDefault();
        }

        this.movingModel.IsPressed = true;

        let clientX = $event.clientX;
        let clientY = $event.clientY;

        if ((<any>window).TouchEvent  && $event instanceof TouchEvent) {
            let touchEvent = <TouchEvent>$event;

            clientX = touchEvent.changedTouches[0].clientX;
            clientY = touchEvent.changedTouches[0].clientY;
        }

        this.movingModel.Offset.Cy = this.phoneModelTemplate.nativeElement.offsetTop - clientY;
        this.movingModel.Offset.Cx = this.phoneModelTemplate.nativeElement.offsetLeft - clientX;
    }

    private template_OnMouseUp = ($event: MouseEvent) => {

        console.log('end');

        if (this.mode === ConstructorModelMode.OnlyView) {
            return;
        }

        this.movingModel.IsPressed = false;

        if($event.cancelable) {
            $event.preventDefault();
        }

        return true;
    }

    private templateOnMouseMove = ($event: MouseEvent) => {

        console.log('move');

        if (this.mode === ConstructorModelMode.OnlyView) {
            return;
        }

        if($event.cancelable) {
            $event.preventDefault();
        }

        if (($event.which === 1 || ((<any>window).TouchEvent  && $event instanceof TouchEvent)) && this.movingModel.IsPressed) {

            let clientX = $event.clientX;
            let clientY = $event.clientY;

            if ((<any>window).TouchEvent  && $event instanceof TouchEvent) {
                let touchEvent = <TouchEvent>$event;

                clientX = touchEvent.changedTouches[0].clientX;
                clientY = touchEvent.changedTouches[0].clientY;
            }

            // Single lazy calculation:
            if (this.borderVector === null) {
                this.calculateBorders();
            }

            var momentTop = Math.round(clientY + this.movingModel.Offset.Cy);
            var momentLeft = Math.round(clientX + this.movingModel.Offset.Cx);

            // Lazy initialization:
            if (this.constructorParams.Template.Offset === null) {
                this.constructorParams.Template.Offset = new Vector2D();
            }

            this.constructorParams.Template.Offset.Cy = MathHelper.ToFrame(momentTop, this.borderVector.Top, this.borderVector.Bottom);
            this.constructorParams.Template.Offset.Cx = MathHelper.ToFrame(momentLeft, this.borderVector.Left, this.borderVector.Right);

            this.updateCssOffsetOfTemplate();
        }
    }

    private isTouchEvent($event: any) {
        return (<any>window).TouchEvent  && $event instanceof TouchEvent;
    }

    //endregion

    //region CSS Updating Rules

    private updateBlankCss() {
        this.blankTemplate.nativeElement.style.height = this.toCssPixelSize(this.constructorParams.BlankTemplate.Height);
    }

    private updateCssOffsetOfTemplate() {
        this.phoneModelTemplate.nativeElement.style.top = this.toCssPixelSize(this.constructorParams.Template.Offset.Cy);
        this.phoneModelTemplate.nativeElement.style.left = this.toCssPixelSize(this.constructorParams.Template.Offset.Cx);
    }

    private updateCssOffsetOfTemplateWithBackground() {
        this.phoneModelWithBackgroundTemplate.nativeElement.style.top = this.toCssPixelSize(this.constructorParams.TemplateWithBackground.Offset.Cy);
        this.phoneModelWithBackgroundTemplate.nativeElement.style.left = this.toCssPixelSize(this.constructorParams.TemplateWithBackground.Offset.Cx);
    }

    private updateCssSizeOfTemplate () {
        this.phoneModelTemplate.nativeElement.style.width = this.toCssPixelSize(this.constructorParams.Template.Width);
        this.phoneModelTemplate.nativeElement.style.height = this.toCssPixelSize(this.constructorParams.Template.Height);
    }

    private updateCssSizeOfTemplateWithBackground () {
        this.phoneModelWithBackgroundTemplate.nativeElement.style.width = this.toCssPixelSize(this.constructorParams.TemplateWithBackground.Width);
        this.phoneModelWithBackgroundTemplate.nativeElement.style.height = this.toCssPixelSize(this.constructorParams.TemplateWithBackground.Height);
    }

    private toCssPixelSize = (d: number) : string => Math.round(d) + 'px';

    //endregion

    //region Size checking

    @HostListener('window:resize', ['$event'])
    onResize(event: any) {
        console.log('resize')
        if(ScreenHelper.IsMobile() === false) {
            console.log('desctop resize')
            this.applySizesForTemplate(true);
            this.applySizesToTemplateWithBackground();
        }
    }

    //endregion
}

//region Private classes

enum ConstructoModelLoadingCode {
    Blank,
    PhoneModel,
    PhoneModelWithBackground
}

class LoadingModel {
    IsBlankLoaded: boolean = false;
    IsPhoneModelLoaded: boolean = false;
    IsPhoneModelWithBackgroundLoaded: boolean = false;

    get AreAllLoaded(): boolean {
        return (
            this.IsBlankLoaded === true &&
            this.IsPhoneModelLoaded === true &&
            this.IsPhoneModelWithBackgroundLoaded === true
        );
    }
}

class BorderVector {
    Left: number;
    Right: number;
    Top: number;
    Bottom: number;
}

class MobilePhoneMovingModel {
    Offset: Vector2D = new Vector2D()
    IsPressed: boolean = false;
}

//endregion

