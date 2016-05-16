/// <reference path="../typings/viz.d.ts" />
/// <reference path="../typings/json-editor.d.ts" />
import {bootstrap}    from '@angular/platform-browser-dynamic';
import {AppComponent} from './app.component';
import {HTTP_PROVIDERS} from '@angular/http'
import 'rxjs/Rx';

bootstrap(AppComponent, [ HTTP_PROVIDERS ]);