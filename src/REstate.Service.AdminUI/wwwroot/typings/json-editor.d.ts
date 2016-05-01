interface JSONEditor {
    new (element: Element, options: JSONEditorOptions),
    
    defaults: any,
    plugins: any
    
}

interface JSONEditorOptions {
    theme?: string,
    iconlib?: string,
    schema: any,
    ajax: boolean
}