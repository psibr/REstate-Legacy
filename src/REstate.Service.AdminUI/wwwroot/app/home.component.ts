import {Component, OnInit} from 'angular2/core';
import {GraphVizComponent} from './graph-viz.component'

interface JSONEditor {
    new (element: Element, options: JSONEditorOptions),
    element: Element,
    options: JSONEditorOptions
}

interface JSONEditorOptions {
    theme?: string,
    schema: any
}

declare var JSONEditor: JSONEditor;

@Component({
    template: `<div id="json-editor"></div><graph-viz [graphVizText] = "graphVizText"></graph-viz>`,
    directives: [GraphVizComponent]
})
export class HomeComponent implements OnInit {
    graphVizText: string = `digraph g { a -> b; }`;
    
    ngOnInit() {
        var element = document.getElementById('json-editor');


        var editor = new JSONEditor(element, {
            theme: 'bootstrap3',
            schema: {
                "$schema": "http://json-schema.org/draft-04/schema#",
                "id": "http://jsonschema.net",
                "type": "object",
                "properties": {
                    "address": {
                        "id": "http://jsonschema.net/address",
                        "type": "object",
                        "properties": {
                            "streetAddress": {
                                "id": "http://jsonschema.net/address/streetAddress",
                                "type": "string"
                            },
                            "city": {
                                "id": "http://jsonschema.net/address/city",
                                "type": "string"
                            }
                        },
                        "required": [
                            "streetAddress",
                            "city"
                        ]
                    },
                    "phoneNumber": {
                        "id": "http://jsonschema.net/phoneNumber",
                        "type": "array",
                        "items": {
                            "id": "http://jsonschema.net/phoneNumber/0",
                            "type": "object",
                            "properties": {
                                "location": {
                                    "id": "http://jsonschema.net/phoneNumber/0/location",
                                    "type": "string"
                                },
                                "code": {
                                    "id": "http://jsonschema.net/phoneNumber/0/code",
                                    "type": "integer"
                                }
                            }
                        }
                    }
                },
                "required": [
                    "address",
                    "phoneNumber"
                ]
            });
    }
}