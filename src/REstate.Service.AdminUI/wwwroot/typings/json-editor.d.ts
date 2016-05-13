interface JSONEditor {
    new (element: Element, options: JSONEditorOptions),
    
    defaults: any,
    plugins: any,
    setValue(value:Object),
    getValue():Object,
    on(event:string, callback:Function)
}

interface JSONEditorOptions {
    theme?: string,
    iconlib?: string,
    schema: any,
    ajax: boolean
}