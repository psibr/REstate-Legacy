interface JSONEditor {
    new (element: Element, options: JSONEditorOptions),
    element: Element,
    options: JSONEditorOptions
}

interface JSONEditorOptions {
    theme?: string,
    iconlib?: string,
    schema: any,
    ajax: boolean
}