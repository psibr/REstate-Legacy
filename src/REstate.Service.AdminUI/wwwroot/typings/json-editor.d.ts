interface JSONEditor {
    new (element: Element, options: JSONEditorOptions),
    element: Element,
    options: JSONEditorOptions
}

interface JSONEditorOptions {
    theme?: string,
    schema: any,
    ajax: boolean
}