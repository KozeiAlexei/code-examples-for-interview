import {Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild} from "@angular/core";
import {BlankService} from "../../../../../core/services/blank/blank.service";
import {Blank} from "../../../../../core/models/blank/blank";
import {BlankFilter} from "../../../../../core/models/blank/blank-filter";
import {ColorScheme} from "../../../../../core/models/blank/color-scheme";
import {ConstructorModelParams} from "../constructor-model/models/constructor-params";
import {CartItem, CartService} from "../../../services/cart-service/cart.service";
import {ScreenHelper} from "../../../../../core/helpers/screen-helper";

@Component({
    selector: 'constructor-grid',
    moduleId: module.id.toString(),
    templateUrl: 'constructor-grid.component.html',
    styles: [
        require('./constructor-grid.component.scss').toString(),
        require('./constructor-grid.mobile.component.scss').toString()
    ]
})
export class ConstructorGridComponent {
    @Input() products: Blank[] = [];
    @Input() constructorParams: ConstructorModelParams;

    @Output() onProductSelect: EventEmitter<Blank> = new EventEmitter();

    @ViewChild('productsContainer', { read: ElementRef })
    private productsElement: ElementRef<any>;

    private isMobile: boolean = ScreenHelper.IsMobile();

    private onProductClick(product: Blank) {
        this.onProductSelect.emit(product);
    }

    private scrollRight(): void {
        this.productsElement.nativeElement.scrollTo({
            left: (this.productsElement.nativeElement.scrollLeft + 150), behavior: 'smooth' });
    }

    private scrollLeft(): void {
        this.productsElement.nativeElement.scrollTo({
            left: (this.productsElement.nativeElement.scrollLeft - 150), behavior: 'smooth' });
    }
}
