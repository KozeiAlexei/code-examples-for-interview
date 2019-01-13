import {BlankBatch} from "../../../../../../core/models/blank/blank-batch";
import {Component, EventEmitter, Input, Output, ViewChild} from "@angular/core";
import {BlankFilter} from "../../../../../../core/models/blank/blank-filter";
import {ConstructorHeaderViewModel} from "../../constructor-header/constructor-header.viewmodel";
import {PagingState} from "../../../../../../core/models/base/paging-options.model";
import {ColorScheme} from "../../../../../../core/models/blank/color-scheme";
import {ComboboxItem, FilterItemComponent} from "../../../../сomponents/filter-item/filter-item.component";
import {PhoneModel} from "../../../../../../core/models/phone-model/phone-model.model";
import {WPH} from "../../../../../../core/helpers/wp-helper";

@Component({
    selector: 'constructor-blank-selector-mobile',
    moduleId: WPH.ModuleSelector(module),
    templateUrl: 'constructor-blank-selector.mobile.component.html',
    styles: [require('./constructor-blank-selector.mobile.component.scss').toString()]
})
export class ConstructorBlankSelectorMobileComponent {

    //region Actions

    SelectBlankBatch = (blankBatchId: number) => this.blankBatchComboBox.SelectById(blankBatchId);

    SelectColorScheme = (colorSchemeId: number) => this.colorSchemesComboBox.SelectById(colorSchemeId);

    UpdateBlankBatches = (blankBatches: BlankBatch[]) => {
        this.blankBatchComboBox.LoadItems(blankBatches.map((blankBatch) => new ComboboxItem(blankBatch.Id, blankBatch.Name)));
        this.blankBatchComboBox.SelectFirst();
    }

    UpdateColorSchemes = (colorSchemes: ColorScheme[]) => {
        this.colorSchemesComboBox.LoadItems(colorSchemes.map((colorScheme) => new ComboboxItem(colorScheme.Id, colorScheme.Name)));
        this.colorSchemesComboBox.SelectFirst();
    }

    //endregion

    //region Attributes

    @Input() viewModel: ConstructorHeaderViewModel;

    @Input() pagingState: PagingState;

    @Input() blankFilter: BlankFilter;

    //endregion

    //region Events

    @Output() onColorSchemeChange: EventEmitter<number> = new EventEmitter();
    @Output() onBlankBatchChange: EventEmitter<number> = new EventEmitter<number>();

    //endregion

    //region Inner Elements

    @ViewChild('blankBatchComboBox')
    private blankBatchComboBox: FilterItemComponent;

    @ViewChild('colorSchemeComboBox')
    private colorSchemesComboBox: FilterItemComponent;

    //endregion
}