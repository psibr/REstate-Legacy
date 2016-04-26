interface Viz {
    (data: string, options: VizOptions): string
}
    
interface VizOptions {
    format: string;
    engine: string;
}