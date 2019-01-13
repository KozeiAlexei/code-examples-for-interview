import {Component, EventEmitter, Input, Output, ViewChild} from "@angular/core";
import {WPH} from "../../../../../../core/helpers/wp-helper";
import {BlankBatch} from "../../../../../../core/models/blank/blank-batch";
import {BlankFilter} from "../../../../../../core/models/blank/blank-filter";
import {ConstructorHeaderViewModel} from "../../constructor-header/constructor-header.viewmodel";
import {PagingState} from "../../../../../../core/models/base/paging-options.model";
import {ColorScheme} from "../../../../../../core/models/blank/color-scheme";
import {ComboboxItem, FilterItemComponent} from "../../../../сomponents/filter-item/filter-item.component";
import {PhoneModel, PhoneModelColoredBlank} from "../../../../../../core/models/phone-model/phone-model.model";

@Component({
    selector: 'constructor-phone-selector-mobile',
    moduleId: WPH.ModuleSelector(module),
    templateUrl: 'constructor-phone-selector.mobile.component.html',
    styles: [require('./constructor-phone-selector.mobile.component.scss').toString()]
})
export class ConstructorPhoneSelectorMobileComponent {

    //region Actions

    SelectPhoneModel = (phoneModelId: number) => this.phoneModelComboBox.SelectById(phoneModelId);

    SelectColor = (coloredBlankId: number) => this.colorsComboBox.SelectById(coloredBlankId);

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

    @Output() onColorChange: EventEmitter<number> = new EventEmitter();
    @Output() onPhoneModelChange: EventEmitter<number> = new EventEmitter();

    //endregion

    //region Inner Elements

    @ViewChild('phoneModelComboBox')
    private phoneModelComboBox: FilterItemComponent;

    @ViewChild('colorsComboBox')
    private colorsComboBox: FilterItemComponent;

    //endregion
}