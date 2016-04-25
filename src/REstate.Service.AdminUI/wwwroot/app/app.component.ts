import {Component} from 'angular2/core';
import { RouteConfig, ROUTER_DIRECTIVES, ROUTER_PROVIDERS } from 'angular2/router';
import {NavbarComponent} from './navbar.component'
import {HomeComponent} from './home.component'
import {AboutComponent} from './about.component'

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
        ROUTER_PROVIDERS
    ]
})
@RouteConfig([
    {
        path: '/',
        name: 'Home',
        component: HomeComponent,
        useAsDefault: true
    },
    {
        path: '/about',
        name: 'About',
        component: AboutComponent
    }
])
export class AppComponent { }