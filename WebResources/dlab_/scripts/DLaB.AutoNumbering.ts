/// <reference path="../../scripts/typings/jquery/jquery.d.ts" />
/// <reference path="../../scripts/typings/xrm/xrm.d.ts" />

// ReSharper disable once InconsistentNaming
module DLaB {
    import BooleanAttribute = Xrm.Page.BooleanAttribute;
    import DateAttribute = Xrm.Page.DateAttribute;
    import LookupAttribute = Xrm.Page.LookupAttribute;
    import NumberAttribute = Xrm.Page.NumberAttribute;
    import OptionSetAttribute = Xrm.Page.OptionSetAttribute;
    import OptionSetValue = Xrm.Page.OptionSetValue;
    import StringAttribute = Xrm.Page.StringAttribute;

    export class AutoNumbering {
        private static instance = new AutoNumbering();

        //#region Form Properties

        static fields = {
            //fieldName: "dlab_FieldName",
            allowExternalInitialization: "dlab_allowexternalinitialization",
            attributeName: "dlab_attributename",
            entityName: "dlab_entityname",
            fixedNumberSize: "dlab_fixednumbersize",
            incrementStepSize: "dlab_incrementstepsize",
            modifiedOn: "modifiedon",
            name: "dlab_name",
            nextNumber: "dlab_nextnumber",
            overridePopulatedIds: "dlab_overridepopulatedids",
            padWithZeros: "dlab_padwithzeros",
            pluginExecutionOrder: "dlab_pluginexecutionorder",
            pluginStepId: "dlab_pluginstepid",
            postfix: "dlab_postfix",
            prefix: "dlab_prefix",
            serverBatchSize: "dlab_serverbatchsize"
        }

        static sections = {}

        static tabs = {}

        static optionSets = {}

        static iFrames = {}

        //#endregion Form Properties

        //#region onLoad / onSave

        static onLoad(): void {
            AutoNumbering.instance.onLoad();
        }

        private onLoad = (): void => {
            if (Xrm.Page.ui.getFormType() === Xrm.Page.FormType.Create) {
                // Default Values
                Common.setValue(AutoNumbering.fields.fixedNumberSize, 8);
                Common.setValue(AutoNumbering.fields.incrementStepSize, 1);
                Common.setValue(AutoNumbering.fields.nextNumber, 0);
                Common.setValue(AutoNumbering.fields.pluginExecutionOrder, 1);
                Common.setValue(AutoNumbering.fields.serverBatchSize, 1);

                Common.setDisabled(AutoNumbering.fields.entityName, false);
                Common.setDisabled(AutoNumbering.fields.attributeName, false);
                Common.setDisabled(AutoNumbering.fields.pluginExecutionOrder, false);
            }

            Common.populateName(AutoNumbering.fields.name, "{0}.{1}", AutoNumbering.fields.entityName, AutoNumbering.fields.attributeName);
            Common.addOnChange(AutoNumbering.fields.padWithZeros, this.showOrHideFixedNumberSize);
            Common.addOnChange(AutoNumbering.fields.modifiedOn, this.onSaveCallback);
        }

        static onSave(): void { AutoNumbering.instance.onSave(); }

        private onSave = (): void => {

        }

        //#endregion onLoad / onSave

        private onSaveCallback = (): void => {
            Common.setDisabled(AutoNumbering.fields.entityName, true);
            Common.setDisabled(AutoNumbering.fields.attributeName, true);
            Common.setDisabled(AutoNumbering.fields.pluginExecutionOrder, true);
        }

        private showOrHideFixedNumberSize = (): void => {
            const isPadded = Common.getValue<BooleanAttribute>(AutoNumbering.fields.padWithZeros);
            Common.setVisible(AutoNumbering.fields.fixedNumberSize, isPadded);

            if (!isPadded) {
                Common.setValue(AutoNumbering.fields.fixedNumberSize, 1);
            }
        }
    }

    export class Common{

        /**
         * Safe Xrm.Page.getAttribute(attributeName).addOnChange()
         *
         * @param attributeName The name of the attribute to subscribe to the onChange event of
         * @param func The Function to call when the Attribute
         */
        static addOnChange(attributeName: string, func: Xrm.Page.ContextSensitiveHandler): void {
            const att = Common.getAttribute(attributeName);
            if (att) {
                att.addOnChange(func);
            }
        }

        //#region setValue Overloads

        /**
         * Safe Xrm.Page.getAttribute(attributeName).setValue(value) for a single Lookup
         * 
         * @param attributeName The name of the attribute to set the value of
         * @param id The GUID of the Lookup
         * @param displayName The Display Name of the Lookup
         * @param logicalName The Logical Entity Name of the Lookup
         */
        static setValue(attributeName: string, id: string, displayName: string, logicalName: string);
        /**
         * Safe Xrm.Page.getAttribute(attributeName).setValue(value) for Lookups
         * 
         * @param attributeName The name of the attribute to set the value of
         * @param lookups The array of Lookups set as selected
         */
        static setValue(attributeName: string, lookups: Xrm.Page.LookupValue[]);
        /**
         * Safe Xrm.Page.getAttribute(attributeName).setValue(value)
         * 
         * @param attributeName The name of the attribute to set the value of
         * @param value The value
         */
        static setValue(attributeName: string, value: boolean | number | string | Date);
        /**
         * Safe Xrm.Page.getAttribute(attributeName).setValue(value)
         * 
         * @param attributeName The name of the attribute to set the value of
         * @param value the value, lookup, lookup array, or GUID for specifiying a lookup
         * @param displayName The display name of the Lookup
         * @param logicalName The logical entity name of Lookup
         */
        static setValue(attributeName: string, value, displayName?: string, logicalName?: string): void {
            if (displayName && logicalName && value && typeof value == "string") {
                // Create Lookup Object Entity Reference
                value = [
                    {
                        id: value,
                        name: displayName,
                        entityType: logicalName
                    }
                ];
            }
            const att = Common.getAttribute(attributeName);
            if (att) {
                (<any>att).setValue(value);
            }
        }

        //#endregion setValue Overloads

        /**
       * Gets the Attribute or returns null if the Attribute is not found or the controlName was null
       * 
       * @param attributeName The name of the attribute to return
       * @returns {} 
       */
        static getAttribute<T extends Xrm.Page.Attribute>(attributeName: string): T {
            if (!attributeName) {
                return null;
            }
            const att = Xrm.Page.getAttribute<T>(attributeName);
            return att ? att : null;
        }

        /**
         * Safe Xrm.Page.getControl()
         *
         * @param controlName The name of the control to return
         * @return The Control
         */
        static getControl<T extends Xrm.Page.Control>(controlName: string): T {
            if (!controlName) {
                return null;
            }
            const ctrl = Xrm.Page.getControl<T>(controlName);
            return ctrl ? ctrl : null;
        }

        /**
         * Safe Xrm.Page.getControl(controlName).getControlType()
         *
         * @param controlName The name of the control to retrieve the control type of
         */
        static getControlType(controlName: string) {
            const ctrl = Common.getControl(controlName);
            return ctrl ? ctrl.getControlType() : null;
        }

        /**
         * Safe Xrm.Page.getAttribute(attributeName).getFormat()
         *
         * @param attributeName The name of the attribute to retrieve the format of
         */
        static getFormat(attributeName: string) {
            const att = Common.getAttribute(attributeName);
            return att ? att.getFormat() : null;
        }

        /**
         * Returns the Text Display value of the control.  Handling if it is a text box, date, lookup, or optionset value.
         * 
         * @param controlName The name of the control to retrieve the display value of
         */
        static getDisplayValue(controlName: string): string {
            const controlType = Common.getControlType(controlName);
            let value;
            switch (controlType) {
                case "optionset":
                    value = Common.getSelectedOptionText(controlName);
                    break;
                case "lookup":
                    value = Common.getSelectedLookupName(controlName);
                    break;
                default:
                    value = Common.getValue(controlName);
                    if (Common.getFormat(controlName) === "date") {
                        value = value.format("MM//dd//yyyy");
                    }
            }
            return value || "";
        }

        /**
         * Similar to the .net String.Format
         * @param format String Format format
         * @param args Args to insert ino the Format String
         */
        static format(text: string, ...args: string[]): string {
            // Check if there are two arguments in the arguments list.
            if (args.length < 1) {

                // If there are not 1 or more arguments there's nothing to replace
                // just return the original text.
                return text;
            }

            for (let i = 0; i < args.length; i++) {

                // Iterate through the tokens and replace their placeholders from the original text in order.
                text = text.replace(new RegExp(`\\{${i}\\}`, "gi"), arguments[i + 1]);
            }

            return text;
        }

        //#region Lookup Control Helpers

        /**
         * Safe Xrm.Page.getAttribute(attributeName).getValue(value)[0] for Lookups
         * 
         * @param attributeName The name of the attribute to set the value of
         * @returns {Lookup} The Single (or First) Selected Lookup
         */
        static getSelectedLookup(attributeName: string): Xrm.Page.LookupValue {
            const value = Common.getValue(attributeName);
            return value ? value[0] : null;
        }

        /**
         * Safe Xrm.Page.getAttribute(attributeName).getValue(value)[0].id for Lookups
         * 
         * @param attributeName The name of the attribute to set the value of
         */
        static getSelectedLookupId(attributeName: string) {
            const lookup = Common.getSelectedLookup(attributeName);
            return lookup ? lookup.id : null;
        }

        /**
         * Safe Xrm.Page.getAttribute(attributeName).getValue(value)[0].name for Lookups
         * @param attributeName The name of the attribute to set the value of
         */
        static getSelectedLookupName(attributeName: string) {
            const lookup = Common.getSelectedLookup(attributeName);
            return lookup ? lookup.name : null;
        }

        /**
         * Safe Xrm.Page.getAttribute(attributeName).setValue(value) for Lookups
         * 
         * @param attributeName The name of the attribute to set the value of
         * @param id The GUID of the EntityReference
         * @param name The display name of the EntityReference
         * @param entityType The Logical Name of the Entity of the EntityReference
         */
        static setLookupValue(attributeName: string, id: string, name: string, entityType: string): void {
            Common.setValue(attributeName, id, name, entityType);
        }

        //#endregion Lookup Control Helpers

        //#region Option Control Helpers

        /**
         * Safe Xrm.Page.getAttribute(attributeName).getSelectedOption()
         * 
         * @param attributeName The name of the attribute to get the selectedOption of
         * @returns {Option} 
         */
        static getSelectedOption(attributeName: string): OptionSetValue {
            const att = Common.getAttribute(attributeName);
            return att ? (<Xrm.Page.OptionSetAttribute>att).getSelectedOption() : null;
        }

        /**
         * Safe Xrm.Page.getAttribute(attributeName).getSelectedOption().text
         * 
         * @param attributeName The name of the attribute to get the text of the selectedOption of
         */
        static getSelectedOptionText(attributeName: string) {
            const selectedOption = Common.getSelectedOption(attributeName);
            return selectedOption ? selectedOption.text : null;
        }

        /**
         * Safe Xrm.Page.getAttribute(attributeName).getSelectedOption().value
         * 
         * @param attributeName The name of the attribute to get the value of the selectedOption of
         */
        static getSelectedOptionValue(attributeName: string): number {
            const selectedOption = Common.getSelectedOption(attributeName);
            if (!selectedOption) {
                // This occurs when you have an asychronous save event that triggers an onChange.  The selectedOption is null, but the get value returns the actual value...
                return Common.getValue(attributeName);
            }
            return (selectedOption.value as any) as number;
        }

        //#endregion Option Control Helpers

        //#region getValue Overloads

        /**
         * Safe Xrm.Page.getAttribute(attributeName).getValue()
         * 
         * @param attributeName The name of the attribute to retrieve the value of
         */
        static getValue(attributeName: string): any;
        // ReSharper disable MoreSpecificSignatureAfterLessSpecific
        /**
         * Safe Xrm.Page.getAttribute(attributeName).getValue()
         * 
         * @param attributeName The name of the attribute to retrieve the value of
         */
        static getValue<T extends BooleanAttribute>(attributeName: string): boolean;
        /**
         * Safe Xrm.Page.getAttribute(attributeName).getValue()
         * 
         * @param attributeName The name of the attribute to retrieve the value of
         */
        static getValue<T extends DateAttribute>(attributeName: string): Date;
        /**
         * Safe Xrm.Page.getAttribute(attributeName).getValue()
         * 
         * @param attributeName The name of the attribute to retrieve the value of
         */
        static getValue<T extends LookupAttribute>(attributeName: string): Xrm.Page.LookupValue[];
        /**
         * Safe Xrm.Page.getAttribute(attributeName).getValue()
         * 
         * @param attributeName The name of the attribute to retrieve the value of
         */
        static getValue<T extends NumberAttribute>(attributeName: string): number;
        /**
         * Safe Xrm.Page.getAttribute(attributeName).getValue()
         * 
         * @param attributeName The name of the attribute to retrieve the value of
         */
        static getValue<T extends OptionSetAttribute>(attributeName: string): OptionSetValue;
        /**
         * Safe Xrm.Page.getAttribute(attributeName).getValue()
         * 
         * @param attributeName The name of the attribute to retrieve the value of
         */
        static getValue<T extends StringAttribute>(attributeName: string): string;
        // ReSharper restore MoreSpecificSignatureAfterLessSpecific
        /**
         * Safe Xrm.Page.getAttribute(attributeName).getValue()
         * 
         * @param attributeName The name of the attribute to retrieve the value of
         */
        static getValue(attributeName: string): any {
            const att = Common.getAttribute(attributeName);
            return att ? (<any>att).getValue() : null;
        }

        //#endregion getValue Overloads

        //#region Populate Name

        /**
         * Will update the name given to the format provided.
         * Performs an additional check for null or empty values and trims them from the end of the format (Assumes the index of the format is sequential, not {0} {3} {2})
         * Call site example: populateName("dlab_name", "{0}: {1} - {2}", "contact", "stateorprovince", "city");
         * 
         * @param nameField The attribute of the field to default
         * @param format The format used to default the name
         */
        static populateName(nameField: string, format: string, ...args: string[]): void {
            if (!Common.getControl(nameField)) {
                return;
            }

            if (args.length === 0) {
                // If there are not any arguments there's nothing to format
                Common.setValue(nameField, format);
                return;
            }

            let hasValue = false;

            // Add Event Handlers
            for (let i = 0; i < args.length; i++) {
                hasValue = hasValue || Common.getValue(args[i]);
                Common.addOnChange(args[i],
                    () => {
                        Common.updateNameHandler(nameField, format, args);
                    });
            }

            // Set Name if at least one value is populated and the name field is not
            if (hasValue && !Common.getValue(nameField)) {
                Common.updateNameHandler(nameField, format, args);
            }

        }

        /**
         * Uses the args array to get field's values to populate the field
         * 
         * @param nameField The attribute of the field to default
         * @param format The format to use to set the string value of
         * @param args String in the format of "Hello {0}
         * @returns {} 
         */
        private static updateNameHandler(nameField: string, format: string, args: string[]) {
            let i: number; // check for ending null values to trim string if need be
            // Allows a format of "{0} - {1} - {2}" to be "Foo", rather than "Foo - - ".  
            for (i = 0; i < args.length - 1; i++) {
                args = <string[]>(args.constructor === Array ? args : [args]);

                if (Common.getValue(args[i])) {
                    continue; // Value not null
                }
                const token = `{${i}}`;
                let startToken = format.indexOf("{");
                let endToken = format.indexOf("}", startToken);
                let index: number;
                if (format.substring(startToken + 1, endToken) === i + "") {
                    // First value in format, remove up to the second
                    index = format.indexOf("{", endToken);
                    if (index >= 0) {
                        format = format.substring(0, startToken) + format.substring(index);
                    }
                }
                else {
                    // Not first value in format, remove up to the previous
                    startToken = format.indexOf(token);
                    endToken = startToken + token.length;
                    index = format.lastIndexOf("}", startToken);
                    if (index > 0) {
                        format = format.substring(0, index + 1) + format.substring(endToken);
                    }
                }
            }

            // Copy array and update format to keep format from being updated in args array for next call
            const formatArgs = args.slice();
            formatArgs.unshift(format);

            for (i = 1; i < formatArgs.length; i++) {
                formatArgs[i] = Common.getDisplayValue(formatArgs[i]);
            }

            Common.setValue(nameField, Common.format.apply(this, formatArgs));
        }

        //#endregion Populate Name

        /**
         * Safe Xrm.Page.getControl(controlName).setDisabled(value)
         * @param controlName Name of the Control to set the disabled of
         * @param disabled True to disable, false to enable
         */
        static setDisabled(controlName: string, disabled = true) {
            const ctrl = Common.getControl(controlName);
            if (ctrl) {
                ctrl.setDisabled(disabled);
            }
        }

        /**
         * Safe Xrm.Page.getControl(attributeName).setVisible(visibility)
         * 
         * @param controlName The name of the control to set the visibility of
         * @param visibility Optional - Defaults to true
         */
        static setVisible(controlName: string, visibility = true): void {

            const ctrl = Common.getControl(controlName);
            if (!ctrl) {
                return;
            }

            ctrl.setVisible(visibility);
        }
    }
}

