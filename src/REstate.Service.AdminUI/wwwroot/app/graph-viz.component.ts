import {Component, OnInit, Input, OnChanges} from '@angular/core';

export interface Viz {
    (data: string, type: string): string
}

declare var Viz: Viz;

@Component({
    template: `<div id="svg-viz" [innerHTML] = "svgText"></div>`,
    selector: 'graph-viz'
})
export class GraphVizComponent implements OnInit, OnChanges {
    @Input()
    graphVizText: string;
    
    svgText: string = '';
    
    ngOnChanges() {
        if(this.graphVizText && this.graphVizText != '') {
            this.svgText = Viz(this.graphVizText, "svg");
        }
    }
    
    ngOnInit() {
        
        if(this.graphVizText && this.graphVizText != '') {
            this.svgText = Viz(this.graphVizText, "svg");
        }
    }
}