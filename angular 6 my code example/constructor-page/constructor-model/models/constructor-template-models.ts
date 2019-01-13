import {Vector2D} from "../../../../../../core/models/math/vector-2d";

export class ConstructorTemplate {
    Width: number;
    Height: number;

    Offset: Vector2D = null;
}

export class ConstructorTemplateWithBackground extends ConstructorTemplate {
    BackgroundFactor: number;
}