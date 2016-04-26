/// <reference path="../typings/viz.d.ts" />
/// <reference path="../typings/json-editor.d.ts" />
import {bootstrap}    from 'angular2/platform/browser';
import {AppComponent} from './app.component';
import {HTTP_PROVIDERS} from 'angular2/http'
import 'rxjs/Rx';

bootstrap(AppComponent, [ HTTP_PROVIDERS ]);