/// <reference path="../typings/json-editor.d.ts" />
import {Component, OnInit} from '@angular/core';
import {ROUTER_DIRECTIVES, ROUTER_PROVIDERS, Router} from '@angular/router-deprecated';
import {REstateService} from './restate.service';
import {MachineDefinition} from './machine-definition';

@Component({
    template: `
        <table class="table table-striped table-bordered">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Initial State</th>
                    <th>Auto Ignore Triggers</th>
                    <th>Created DateTime</th>
                </tr>    
            </thead>
            <tbody>
                <tr *ngFor="let definition of definitions" (click)="onSelect(definition)">
                    <td>{{definition.machineName}}</td>
                    <td>{{definition.initialState}}</td>
                    <td>{{definition.autoIgnoreTriggers}}</td>
                    <td>{{definition.createdDateTime}}</td>
                </tr>
            </tbody>
        </table>`,
    directives: [ROUTER_DIRECTIVES],
    selector: 'machine-list'
})
export class MachineListComponent implements OnInit {
    definitions: MachineDefinition[];
    errorMessage: string;
    
    constructor (private router:Router, private REstateService: REstateService) { }
    
    onSelect(machine:MachineDefinition) {
        this.router.navigate(['MachineEditor', { machineName: machine.machineName}]);
    }
    
    ngOnInit() {
        
        this.REstateService.getMachineDefinitions()
            .subscribe(
                definitions => this.definitions = definitions,
                error => this.errorMessage = error
            );
    }
}