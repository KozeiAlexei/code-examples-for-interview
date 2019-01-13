import {ComboboxItem} from "../../../сomponents/filter-item/filter-item.component";
import {BlankBatch} from "../../../../../core/models/blank/blank-batch";
import {ColorScheme} from "../../../../../core/models/blank/color-scheme";
import {BlankFilter} from "../../../../../core/models/blank/blank-filter";
import {PhoneModel, PhoneModelColoredBlank} from "../../../../../core/models/phone-model/phone-model.model";

export class ConstructorHeaderViewModel {

    private _blankBatches: ComboboxItem[];
    private _colorSchemes: ComboboxItem[];
    private _phoneModels: ComboboxItem[];
    private _colors: ComboboxItem[];

    private _phoneCaseBlanksOriginal: PhoneModel[];

    constructor(blankBatches: BlankBatch[], colorSchemes: ColorScheme[], phoneModels: PhoneModel[]) {
        this._blankBatches = blankBatches.map((blankBatch) => new ComboboxItem(blankBatch.Id, blankBatch.Name));
        this._colorSchemes = colorSchemes.map((colorScheme) => new ComboboxItem(colorScheme.Id, colorScheme.Name));
        this._phoneModels = phoneModels.map(
                (phoneModel) => new ComboboxItem(phoneModel.Id, `${phoneModel.PhoneManufacturer.Name} ${phoneModel.Name}`)
        );

        this._phoneCaseBlanksOriginal = phoneModels;
        this._colors = [];
    }

    get BlankBatches() : ComboboxItem[] {
        return this._blankBatches;
    }

    get ColorSchemes() : ComboboxItem[] {
        return this._colorSchemes;
    }

    get PhoneModels() : ComboboxItem[] {
        return this._phoneModels;
    }

    get Colors() : ComboboxItem[] {
        return this._colors;
    }

    DefaultBlankBatchId: number = null;
    DefaultColorSchemeId: number = null;
    DefaultPhoneModelId: number = null;
    DefaultColorId: number = null;

    AddFilter = (filter: BlankFilter) => {
        this.DefaultBlankBatchId = filter.BatchId;
        this.DefaultColorSchemeId = filter.ColorSchemeId;
        this.DefaultPhoneModelId = filter.PhoneModelId;
        this.DefaultColorId = filter.PhoneModelColoredBlankId;

        this._colors =
            this._phoneCaseBlanksOriginal
                .find((blank: PhoneModel) => blank.Id === filter.PhoneModelId)
                .AvailableColors
                .map((phoneCaseColoredBlank: PhoneModelColoredBlank) =>
                    new ComboboxItem(phoneCaseColoredBlank.Id, phoneCaseColoredBlank.Color.Name, phoneCaseColoredBlank)
                );
    }
}