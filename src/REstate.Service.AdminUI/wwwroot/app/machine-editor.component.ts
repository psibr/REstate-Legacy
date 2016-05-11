import {Component, OnInit, Input} from 'angular2/core';
import {REstateService} from './restate.service';
import {RouteParams} from 'angular2/router'
import {Machine} from './machine'

declare var JSONEditor: JSONEditor;

@Component({
    template: `
    <div id="json-editor-fork-btn" style="display: none;">
        <div class="btn-group" style="margin-left: 10px;"><button class="btn btn-sm btn-raised btn-primary json-editor-btn-edit" type="button">Fork</button><div style="border: 1px solid black; box-shadow: black 3px 3px; position: absolute; z-index: 10; display: none; background-color: white;"><div class="property-selector" style="width: 295px; max-height: 160px; padding: 5px 0px 5px 5px; overflow-y: auto; overflow-x: hidden;"></div><input type="text" class="form-control" placeholder="Property name..." style="width: 220px; margin-bottom: 0px; display: inline-block;"><button type="button" title="add" class="btn btn-sm btn-default btn-raised json-editor-btn-add "><i class="glyphicon glyphicon-plus"></i> add</button><div style="clear: both;"></div></div></div>
    </div>
    <div id="json-editor"></div>`,
    selector: 'machine-editor'
})
export class MachineEditorComponent implements OnInit {
    jsonEditor: JSONEditor;
    machine: Object;

    constructor(private routeParams: RouteParams, private REstateService: REstateService) { }

    ngOnInit() {

        var element = document.getElementById('json-editor');
        let machineName = this.routeParams.get('machineName');

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
                                this.jsonEditor.setValue(this.machine);
                                this.jsonEditor.disable();
                            }).bind(this));
                    }
                    
                    document.getElementById("json-editor")
                        .firstElementChild
                        .firstElementChild
                        .appendChild(document.getElementById("json-editor-fork-btn")
                            .firstElementChild);
                            
                }.bind(this));
            });
    }
}