export class Machine {
    machineName: string;
    initialState: string;
    autoIgnoreTriggers: string;
    stateConfigurations: StateConfiguration[];
}

export class StateConfiguration {
    stateName: string;
    parentStateName: string;
    stateDescription: string;
    transitions: Transition[];
    onEntry: Code;
    ignoreRules: string[];
    onExit: Code;
    onEntryFrom: OnEntryFrom[];
}
    
export class Transition {
    triggerName: string;
    resultantStateName: string;
    guard: Code;
}

export class Code {
    name: string;
    connectorKey: string;
    body: string;
    description: string;
    options: Object;
}

export class OnEntryFrom {
    name: string;
    triggerName: string;
    connectorKey: string;
    body: string;
    description: string;
    options: Object;
}