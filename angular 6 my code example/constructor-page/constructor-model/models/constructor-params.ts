import {Blank} from "../../../../../../core/models/blank/blank";
import {Vector2D} from "../../../../../../core/models/math/vector-2d";
import {ConstructorTemplateWithBackground} from "./constructor-template-models";
import {ConstructorTemplate} from "./constructor-template-models";
import {PhoneModel, PhoneModelColoredBlank} from "../../../../../../core/models/phone-model/phone-model.model";

export class ConstructorModelParams {
    Blank: Blank;
    PhoneModel: PhoneModel;

    PhoneModelColoredBlank: PhoneModelColoredBlank;
    PhoneModelColoredBlankId: number = null;

    PushPhoneModelColoredCase = () => {
        if(this.PhoneModelColoredBlankId === null) {
            if (this.PhoneModel.AvailableColors.length > 0) {
                this.PhoneModelColoredBlankId = this.PhoneModel.AvailableColors[0].Id;
            }
        }

        this.PhoneModelColoredBlank = this.PhoneModel.AvailableColors.find(blank => blank.Id === this.PhoneModelColoredBlankId);
    }

    IsBlankRotated: boolean = false;

    BlankTemplate: ConstructorTemplate = new ConstructorTemplate();

    Template: ConstructorTemplate = new ConstructorTemplate();
    TemplateWithBackground: ConstructorTemplateWithBackground = new ConstructorTemplateWithBackground();

    Clear = () => {
        this.Template = new ConstructorTemplate();
        this.TemplateWithBackground = new ConstructorTemplateWithBackground();

        this.PhoneModelColoredBlankId = null;

        this.templateScaleVector = null;
    }

    get TemplateScaleVector(): Vector2D {
        if (this.templateScaleVector === null) {
            this.templateScaleVector = new Vector2D();

            this.templateScaleVector.Cx = this.PhoneModelColoredBlank.Template.Width / this.Template.Width;
            this.templateScaleVector.Cy = this.PhoneModelColoredBlank.Template.Height / this.Template.Height;
        }

        return this.templateScaleVector;
    }

    static Restore(params: ConstructorModelParams) {
        let restored = new ConstructorModelParams();

        restored.Blank = params.Blank;
        restored.Template = params.Template;
        restored.TemplateWithBackground = params.TemplateWithBackground;

        restored.PhoneModel = new PhoneModel(params.PhoneModel);
        restored.PhoneModelColoredBlankId = params.PhoneModelColoredBlankId;
        restored.PushPhoneModelColoredCase();

        restored.IsBlankRotated = params.IsBlankRotated;

        return restored;
    }

    Clone = (): ConstructorModelParams => {
        return ConstructorModelParams.Restore(JSON.parse(JSON.stringify(this)));
    }

    private templateScaleVector: Vector2D = null;
}