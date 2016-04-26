/// <reference path="../typings/viz.d.ts" />
import {Component, OnInit, Input} from 'angular2/core';

declare var Viz: Viz;

@Component({
    template: `<div id="svg-viz" [innerHTML] = "svgText"></div>`,
    selector: 'graph-viz'
})
export class GraphVizComponent implements OnInit {
    @Input()
    graphVizText: string;
    
    svgText: string;
    
    ngOnInit() {
        
        this.svgText = Viz(this.graphVizText, { format: "svg", engine: "dot" });
    }
}