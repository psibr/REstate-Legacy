import {Injectable} from '@angular/core'
import {Http, Response} from '@angular/http';
import {Observable} from 'rxjs/Observable';
import {MachineDefinition} from './machine-definition';
import {Machine} from './machine'

@Injectable()
export class REstateService {
    constructor (private http: Http) {}
    
    getMachineDefinitions () : Observable<MachineDefinition[]> {
        return this.http.get("api/machines/")
            .map(this.extractData)
            .catch(this.handleError)
    }
    
    getMachineSchema () : Observable<Object> {
        return this.http.get("api/machine")
            .map(this.extractData)
            .catch(this.handleError)
    }
    
    getMachineDefinition (machineName:string) : Observable<Object> {
        return this.http.get("api/machines/" + machineName)
            .map(this.extractAndPrune.bind(this))
            .catch(this.handleError)
    }
    
    private extractData(res: Response) {
        if (res.status < 200 || res.status >= 300) {
        throw new Error('Bad response status: ' + res.status);
        }
        let body = res.json();
        return body || { };
    }
    
    private pruneData(res: Object) {
        this.walkJSONObj(res);
        return res;
    }
    
    private walkJSONObj(obj){
        for (var prop in obj) {
            if(obj[prop] === null) {
                delete obj[prop]
            }
            else{
                if (obj[prop].constructor === Array) {        //if prop is an array 
                    this.walkJSONArray(obj[prop]);                   //go walk through the arrays
                    obj[prop].length === 0 && delete obj[prop]; //if on return the array is empty delete it
                }
                else typeof obj[prop] === 'undefined' && delete obj[prop]; //delete undefined props
            }
        }    
    }

    private walkJSONArray(arr){
        for (var l = arr.length-1; l >= 0; l--) {
            this.walkJSONObj(arr[l]); // go walk the item objects
            if (Object.keys(arr[l]).length === 0 && JSON.stringify(arr[l]) === JSON.stringify({})) {
                arr.splice(l, 1);  // if on return the object is empty delete it
            }
        }
    }
    
    private extractAndPrune(res: Response) {
        let result = this.extractData(res);
        
        result = this.pruneData(result);
        
        return result;        
    }
    
    private handleError (error: any) {
        // In a real world app, we might send the error to remote logging infrastructure
        let errMsg = error.message || 'Server error';
        console.error(errMsg); // log to console instead
        return Observable.throw(errMsg);
    }
}