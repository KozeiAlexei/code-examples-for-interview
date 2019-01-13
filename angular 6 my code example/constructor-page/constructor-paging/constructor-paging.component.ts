import {Component, EventEmitter, Input, Output} from "@angular/core";
import {WPH} from "../../../../../core/helpers/wp-helper";
import {PagingState} from "../../../../../core/models/base/paging-options.model";

@Component({
    selector: 'constructor-paging',
    moduleId: WPH.ModuleSelector(module),
    templateUrl: 'constructor-paging.component.html'
})
export class ConstructorPagingComponent {

    //region Actions

    UpdateFrame = () => {
        this.frame.From = this.state.Size * (this.state.CurrentNumber - 1);
        this.frame.To = this.frame.From + this.state.Size;

        this.frame.From++;

        if(this.frame.To > this.state.AllElementCount) {
            this.frame.To = this.state.AllElementCount;
        }
    }

    //endregion

    //region Attributes

    @Input()
    set pagingState(value: PagingState) {
        if(!value) {
            return;
        }

        this.state = value;
        this.frame = new PagingFrame();

        this.UpdateFrame();
    }

    //endregion

    //region Events

    @Output() onPageChange: EventEmitter<any> = new EventEmitter();

    //endregion

    //region Private Members

    private state: PagingState;
    private frame: PagingFrame;

    private nextPage = () => {
        this.state.CurrentNumber++;
        this.UpdateFrame();

        this.onPageChange.emit();
    }

    private prevPage = () => {
        this.state.CurrentNumber--;
        this.UpdateFrame();

        this.onPageChange.emit();
    }

    //endregion
}

class PagingFrame {
    To: number = 0;
    From: number = 0;
}

