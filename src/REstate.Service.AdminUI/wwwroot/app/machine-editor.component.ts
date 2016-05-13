import {Component, OnInit, Input} from 'angular2/core';
import {REstateService} from './restate.service';
import {RouteParams, Router} from 'angular2/router'
import {Machine} from './machine'

declare var JSONEditor: JSONEditor;

@Component({
    template: `
    <div id="json-editor-extra-btns" style="display: none;">
        <span>
            <button class="btn btn-sm btn-raised btn-primary json-editor-btn-edit" id="btn-preview" type="button">Preview</button>
            <div style="border: 1px solid black; box-shadow: black 3px 3px; position: absolute; z-index: 10; display: none; background-color: white;">
                <div style="clear: both;"></div>
            </div>
        <div class="btn-group">
            <button class="btn btn-sm btn-raised btn-primary json-editor-btn-edit" id="btn-fork" type="button" (click)="fork()">Fork</button>
            <button class="btn btn-sm btn-raised btn-primary json-editor-btn-edit" id="btn-create" type="button">Create</button>
            <div style="border: 1px solid black; box-shadow: black 3px 3px; position: absolute; z-index: 10; display: none; background-color: white;">
                <div style="clear: both;"></div>
            </div>
        </div>
        </span>
    </div>
    <div id="json-editor"></div>`,
    selector: 'machine-editor'
})
export class MachineEditorComponent implements OnInit {
    jsonEditor: JSONEditor;
    machine: Object;

    constructor(private router:Router, private routeParams: RouteParams, private REstateService: REstateService) { }

    ngOnInit() {

        var element = document.getElementById('json-editor');
        let machineName = this.routeParams.get('machineName');
        let forkGuid = this.routeParams.get('forkGuid');
        
        this.REstateService.getMachineSchema()
            .subscribe(
            schema => {
                this.jsonEditor = new JSONEditor(element, {
                    theme: 'material',
                    iconlib: "bootstrap3",
                    ajax: true,
                    schema: schema
                });

                this.jsonEditor.on('ready', function () {
                    
                    if (machineName) {
                        this.REstateService.getMachineDefinition(machineName)
                            .subscribe((
                            machine => {
                                this.machine = machine;
                                (<HTMLButtonElement>document.getElementById("btn-create")).disabled = true;
                                this.jsonEditor.setValue(this.machine);
                                this.jsonEditor.disable();
                            }).bind(this));
                    }
                    else if (forkGuid) {
                        var json = sessionStorage.getItem(forkGuid);
                        
                        this.machine = JSON.parse(json);
                        this.machine.machineName = null;
                        
                        (<HTMLButtonElement>document.getElementById("btn-fork")).disabled = true;
                        
                        this.jsonEditor.setValue(this.machine);
                    }
                    else
                    {
                        (<HTMLButtonElement>document.getElementById("btn-fork")).disabled = true;
                    }
                    
                    document.getElementById("json-editor")
                        .firstElementChild
                        .firstElementChild
                        .appendChild(document.getElementById("json-editor-extra-btns")
                            .firstElementChild);
                            
                }.bind(this));
            });
    }
    
    fork() {
        var forkGuid:string = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            var r = Math.random()*16|0, v = c == 'x' ? r : (r&0x3|0x8);
            return v.toString(16);
        });
        
        var jsonValue:string = JSON.stringify(this.jsonEditor.getValue());
                
        sessionStorage.setItem(forkGuid, jsonValue);
        
        this.router.navigate(['MachineEditor', { forkGuid: forkGuid }]);
    }
}