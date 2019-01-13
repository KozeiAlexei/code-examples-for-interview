///<reference path="../../../../../../node_modules/@angular/core/src/metadata/lifecycle_hooks.d.ts"/>
import {
    Component, EventEmitter, Input, Output, ViewChild
} from "@angular/core";
import {BlankFilter} from "../../../../../core/models/blank/blank-filter";
import {PagingState} from "../../../../../core/models/base/paging-options.model";
import {BlankBatch} from "../../../../../core/models/blank/blank-batch";
import {ColorScheme} from "../../../../../core/models/blank/color-scheme";
import {FilterItemComponent, ComboboxItem} from "../../../сomponents/filter-item/filter-item.component";
import {ConstructorHeaderViewModel} from "./constructor-header.viewmodel";
import {PhoneModel, PhoneModelColoredBlank} from "../../../../../core/models/phone-model/phone-model.model";

@Component({
    selector: 'constructor-header',
    moduleId: module.id.toString(),
    templateUrl: 'constructor-header.component.html'
})
export class ConstructorHeaderComponent {

    //region Actions

    SelectBlankBatch = (blankBatchId: number) => this.blankBatchComboBox.SelectById(blankBatchId);

    SelectColorScheme = (colorSchemeId: number) => this.colorSchemesComboBox.SelectById(colorSchemeId);

    SelectPhoneModel = (phoneModelId: number) => this.phoneModelComboBox.SelectById(phoneModelId);

    SelectColor = (coloredBlankId: number) => this.colorsComboBox.SelectById(coloredBlankId);

    UpdateBlankBatches = (blankBatches: BlankBatch[]) => {
        this.blankBatchComboBox.LoadItems(blankBatches.map((blankBatch) => new ComboboxItem(blankBatch.Id, blankBatch.Name)));
        this.blankBatchComboBox.SelectFirst();
    }

    UpdateColorSchemes = (colorSchemes: ColorScheme[]) => {
        this.colorSchemesComboBox.LoadItems(colorSchemes.map((colorScheme) => new ComboboxItem(colorScheme.Id, colorScheme.Name)));
        this.colorSchemesComboBox.SelectFirst();
    }

    UpdatePhoneModels = (phoneModels: PhoneModel[]) => {
        this.phoneModelComboBox.LoadItems(phoneModels.map(
            (phoneModel) => new ComboboxItem(phoneModel.Id, `${phoneModel.PhoneManufacturer.Name} ${phoneModel.Name}`)
        ));
        this.phoneModelComboBox.SelectFirst();
    }

    UpdateColors = (colors: PhoneModelColoredBlank[]) => {
        this.colorsComboBox.LoadItems(colors.map((blank: PhoneModelColoredBlank) =>
            new ComboboxItem(blank.Id, blank.Color.Name, blank)
        ));

        this.colorsComboBox.SelectFirst();
    }

    //endregion

    //region Attributes

    @Input() viewModel: ConstructorHeaderViewModel;

    @Input() pagingState: PagingState;

    @Input() blankFilter: BlankFilter;

    //endregion

    //region Events

    @Output() onBlankBatchChange: EventEmitter<number> = new EventEmitter<number>();

    @Output() onColorSchemeChange: EventEmitter<number> = new EventEmitter();

    @Output() onPhoneModelChange: EventEmitter<number> = new EventEmitter();

    @Output() onColorChange: EventEmitter<number> = new EventEmitter();


    //endregion

    //region Inner Elements

    @ViewChild('blankBatchComboBox')
    private blankBatchComboBox: FilterItemComponent;

    @ViewChild('phoneModelComboBox')
    private phoneModelComboBox: FilterItemComponent;

    @ViewChild('colorSchemeComboBox')
    private colorSchemesComboBox: FilterItemComponent;

    @ViewChild('colorsComboBox')
    private colorsComboBox: FilterItemComponent;

    //endregion
}