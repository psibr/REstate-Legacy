import {Component} from '@angular/core';
import { RouteConfig, ROUTER_DIRECTIVES, ROUTER_PROVIDERS } from '@angular/router-deprecated';
import {NavbarComponent} from './navbar.component'
import {HomeComponent} from './home.component'
import {AboutComponent} from './about.component'
import {MachineListComponent} from './machine-list.component';
import {MachineEditorComponent} from './machine-editor.component';
import {REstateService} from './restate.service';

@Component({
    selector: 'restate-admin',
    template: `
        <navbar></navbar>
        <div class="container">
            <router-outlet></router-outlet>
        </div>`,
    directives: [
        ROUTER_DIRECTIVES, 
        NavbarComponent
    ],
    providers: [
        ROUTER_PROVIDERS,
        REstateService
    ]
})
@RouteConfig([
    {
        path: '/',
        name: 'Home',
        component: HomeComponent
    },
    {
        path: '/about',
        name: 'About',
        component: AboutComponent
    },
    {
        path: '/machines',
        name: 'MachineList',
        component: MachineListComponent
    },
    {
        path: '/machines/editor',
        name: 'MachineEditor',
        component: MachineEditorComponent
    }
    
])
export class AppComponent { }