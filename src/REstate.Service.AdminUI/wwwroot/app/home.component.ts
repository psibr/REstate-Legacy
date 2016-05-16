/// <reference path="../typings/json-editor.d.ts" />
import {Component, OnInit} from '@angular/core';
import {GraphVizComponent} from './graph-viz.component';
import {REstateService} from './restate.service';
import {MachineDefinition} from './machine-definition';
import {MachineEditorComponent} from './machine-editor.component';
import {MachineListComponent} from './machine-list.component';

@Component({
    template: `
        <machine-list></machine-list>
        <machine-editor></machine-editor>
        <graph-viz [graphVizText] = "graphVizText"></graph-viz>`,
    directives: [MachineListComponent, GraphVizComponent, MachineEditorComponent]
})
export class HomeComponent implements OnInit {
    graphVizText: string = `digraph {
 AutoRotateFinished -> BatchUnlockReceived [label="Receive"];
 AutoRotateReceived -> AutoRotateFinished [label="Finish"];
 BatchUnlockReceived -> Completed [label="Finish"];
 Configured -> AutoRotateReceived [label="Receive"];
 Created -> Configured [label="NotifyConfigured"];
node [shape=box];
 AutoRotateFinished -> "Indicates batch is now ready for unlocking." [label="On Entry" style=dotted];
 Configured -> "Indicates work ready for AutoRotate." [label="On Entry" style=dotted];
}`;
    
    ngOnInit() {
    }
}