interface JSONEditor {
    new (element: Element, options: JSONEditorOptions),
    
    defaults: any
    
}

interface JSONEditorOptions {
    theme?: string,
    iconlib?: string,
    schema: any,
    ajax: boolean
}