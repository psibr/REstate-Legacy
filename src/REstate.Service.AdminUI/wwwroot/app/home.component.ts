/// <reference path="../typings/json-editor.d.ts" />
import {Component, OnInit} from 'angular2/core';
import {GraphVizComponent} from './graph-viz.component';
import {REstateService} from './restate.service';
import {MachineDefinition} from './machine-definition';
import { ROUTER_DIRECTIVES } from 'angular2/router';

declare var JSONEditor: JSONEditor;

@Component({
    template: `
        <table class="table table-striped table-bordered">
            <thead>
                <tr>
                    <th>Id</th>
                    <th>Name</th>
                    <th>Description</th>
                    <th>Initial State</th>
                    <th>Auto Ignore</th>
                    <th>Active</th>
                </tr>    
            </thead>
            <tbody>
                <tr *ngFor="#definition of definitions" [routerLink]="['About']">
                    <td>{{definition.machineDefinitionId}}</td>
                    <td>{{definition.machineName}}</td>
                    <td>{{definition.machineDescription}}</td>
                    <td>{{definition.initialStateName}}</td>
                    <td>{{definition.autoIgnoreNotConfiguredTriggers}}</td>
                    <td>{{definition.isActive}}</td>
                </tr>
            </tbody>
        </table>
        <div id="json-editor"></div>
        <graph-viz [graphVizText] = "graphVizText"></graph-viz>`,
    directives: [GraphVizComponent, ROUTER_DIRECTIVES],
    providers: [REstateService]
})
export class HomeComponent implements OnInit {
    graphVizText: string = `digraph g { a -> b; }`;
    definitions: MachineDefinition[];
    schema: Object;
    errorMessage: string;
    
    constructor (private REstateService: REstateService) { }
    
    ngOnInit() {
        
        JSONEditor.defaults.options.object_layout = "grid";
        
        var element = document.getElementById('json-editor');
        
        this.REstateService.getMachineDefinitions()
            .subscribe(
                definitions => this.definitions = definitions,
                error => this.errorMessage = error
            );
            
        this.REstateService.getMachineSchema()
            .subscribe(
                schema => new JSONEditor(element, {
                    theme: 'material',
                    iconlib: "bootstrap3",
                    ajax: true,
                    schema: schema
                }),
                error => this.errorMessage = error
            );
    }
}