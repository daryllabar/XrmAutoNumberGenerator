/// <reference path="../../scripts/typings/jquery/jquery.d.ts" />
/// <reference path="../../scripts/typings/xrm/xrm.d.ts" />
// ReSharper disable once InconsistentNaming
var DLaB;
(function (DLaB) {
    var AutoNumbering = (function () {
        function AutoNumbering() {
            var _this = this;
            this.onLoad = function () {
                if (Xrm.Page.ui.getFormType() === 1 /* Create */) {
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
                Common.addOnChange(AutoNumbering.fields.padWithZeros, _this.showOrHideFixedNumberSize);
                Common.addOnChange(AutoNumbering.fields.modifiedOn, _this.onSaveCallback);
            };
            this.onSave = function () {
            };
            //#endregion onLoad / onSave
            this.onSaveCallback = function () {
                Common.setDisabled(AutoNumbering.fields.entityName, true);
                Common.setDisabled(AutoNumbering.fields.attributeName, true);
                Common.setDisabled(AutoNumbering.fields.pluginExecutionOrder, true);
            };
            this.showOrHideFixedNumberSize = function () {
                var isPadded = Common.getValue(AutoNumbering.fields.padWithZeros);
                Common.setVisible(AutoNumbering.fields.fixedNumberSize, isPadded);
                if (!isPadded) {
                    Common.setValue(AutoNumbering.fields.fixedNumberSize, 1);
                }
            };
        }
        //#endregion Form Properties
        //#region onLoad / onSave
        AutoNumbering.onLoad = function () {
            AutoNumbering.instance.onLoad();
        };
        AutoNumbering.onSave = function () { AutoNumbering.instance.onSave(); };
        AutoNumbering.instance = new AutoNumbering();
        //#region Form Properties
        AutoNumbering.fields = {
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
        };
        AutoNumbering.sections = {};
        AutoNumbering.tabs = {};
        AutoNumbering.optionSets = {};
        AutoNumbering.iFrames = {};
        return AutoNumbering;
    })();
    DLaB.AutoNumbering = AutoNumbering;
    var Common = (function () {
        function Common() {
        }
        /**
         * Safe Xrm.Page.getAttribute(attributeName).addOnChange()
         *
         * @param attributeName The name of the attribute to subscribe to the onChange event of
         * @param func The Function to call when the Attribute
         */
        Common.addOnChange = function (attributeName, func) {
            var att = Common.getAttribute(attributeName);
            if (att) {
                att.addOnChange(func);
            }
        };
        /**
         * Safe Xrm.Page.getAttribute(attributeName).setValue(value)
         *
         * @param attributeName The name of the attribute to set the value of
         * @param value the value, lookup, lookup array, or GUID for specifiying a lookup
         * @param displayName The display name of the Lookup
         * @param logicalName The logical entity name of Lookup
         */
        Common.setValue = function (attributeName, value, displayName, logicalName) {
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
            var att = Common.getAttribute(attributeName);
            if (att) {
                att.setValue(value);
            }
        };
        //#endregion setValue Overloads
        /**
       * Gets the Attribute or returns null if the Attribute is not found or the controlName was null
       *
       * @param attributeName The name of the attribute to return
       * @returns {}
       */
        Common.getAttribute = function (attributeName) {
            if (!attributeName) {
                return null;
            }
            var att = Xrm.Page.getAttribute(attributeName);
            return att ? att : null;
        };
        /**
         * Safe Xrm.Page.getControl()
         *
         * @param controlName The name of the control to return
         * @return The Control
         */
        Common.getControl = function (controlName) {
            if (!controlName) {
                return null;
            }
            var ctrl = Xrm.Page.getControl(controlName);
            return ctrl ? ctrl : null;
        };
        /**
         * Safe Xrm.Page.getControl(controlName).getControlType()
         *
         * @param controlName The name of the control to retrieve the control type of
         */
        Common.getControlType = function (controlName) {
            var ctrl = Common.getControl(controlName);
            return ctrl ? ctrl.getControlType() : null;
        };
        /**
         * Safe Xrm.Page.getAttribute(attributeName).getFormat()
         *
         * @param attributeName The name of the attribute to retrieve the format of
         */
        Common.getFormat = function (attributeName) {
            var att = Common.getAttribute(attributeName);
            return att ? att.getFormat() : null;
        };
        /**
         * Returns the Text Display value of the control.  Handling if it is a text box, date, lookup, or optionset value.
         *
         * @param controlName The name of the control to retrieve the display value of
         */
        Common.getDisplayValue = function (controlName) {
            var controlType = Common.getControlType(controlName);
            var value;
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
        };
        /**
         * Similar to the .net String.Format
         * @param format String Format format
         * @param args Args to insert ino the Format String
         */
        Common.format = function (text) {
            var args = [];
            for (var _i = 1; _i < arguments.length; _i++) {
                args[_i - 1] = arguments[_i];
            }
            // Check if there are two arguments in the arguments list.
            if (args.length < 1) {
                // If there are not 1 or more arguments there's nothing to replace
                // just return the original text.
                return text;
            }
            for (var i = 0; i < args.length; i++) {
                // Iterate through the tokens and replace their placeholders from the original text in order.
                text = text.replace(new RegExp("\\{" + i + "\\}", "gi"), arguments[i + 1]);
            }
            return text;
        };
        //#region Lookup Control Helpers
        /**
         * Safe Xrm.Page.getAttribute(attributeName).getValue(value)[0] for Lookups
         *
         * @param attributeName The name of the attribute to set the value of
         * @returns {Lookup} The Single (or First) Selected Lookup
         */
        Common.getSelectedLookup = function (attributeName) {
            var value = Common.getValue(attributeName);
            return value ? value[0] : null;
        };
        /**
         * Safe Xrm.Page.getAttribute(attributeName).getValue(value)[0].id for Lookups
         *
         * @param attributeName The name of the attribute to set the value of
         */
        Common.getSelectedLookupId = function (attributeName) {
            var lookup = Common.getSelectedLookup(attributeName);
            return lookup ? lookup.id : null;
        };
        /**
         * Safe Xrm.Page.getAttribute(attributeName).getValue(value)[0].name for Lookups
         * @param attributeName The name of the attribute to set the value of
         */
        Common.getSelectedLookupName = function (attributeName) {
            var lookup = Common.getSelectedLookup(attributeName);
            return lookup ? lookup.name : null;
        };
        /**
         * Safe Xrm.Page.getAttribute(attributeName).setValue(value) for Lookups
         *
         * @param attributeName The name of the attribute to set the value of
         * @param id The GUID of the EntityReference
         * @param name The display name of the EntityReference
         * @param entityType The Logical Name of the Entity of the EntityReference
         */
        Common.setLookupValue = function (attributeName, id, name, entityType) {
            Common.setValue(attributeName, id, name, entityType);
        };
        //#endregion Lookup Control Helpers
        //#region Option Control Helpers
        /**
         * Safe Xrm.Page.getAttribute(attributeName).getSelectedOption()
         *
         * @param attributeName The name of the attribute to get the selectedOption of
         * @returns {Option}
         */
        Common.getSelectedOption = function (attributeName) {
            var att = Common.getAttribute(attributeName);
            return att ? att.getSelectedOption() : null;
        };
        /**
         * Safe Xrm.Page.getAttribute(attributeName).getSelectedOption().text
         *
         * @param attributeName The name of the attribute to get the text of the selectedOption of
         */
        Common.getSelectedOptionText = function (attributeName) {
            var selectedOption = Common.getSelectedOption(attributeName);
            return selectedOption ? selectedOption.text : null;
        };
        /**
         * Safe Xrm.Page.getAttribute(attributeName).getSelectedOption().value
         *
         * @param attributeName The name of the attribute to get the value of the selectedOption of
         */
        Common.getSelectedOptionValue = function (attributeName) {
            var selectedOption = Common.getSelectedOption(attributeName);
            if (!selectedOption) {
                // This occurs when you have an asychronous save event that triggers an onChange.  The selectedOption is null, but the get value returns the actual value...
                return Common.getValue(attributeName);
            }
            return selectedOption.value;
        };
        // ReSharper restore MoreSpecificSignatureAfterLessSpecific
        /**
         * Safe Xrm.Page.getAttribute(attributeName).getValue()
         *
         * @param attributeName The name of the attribute to retrieve the value of
         */
        Common.getValue = function (attributeName) {
            var att = Common.getAttribute(attributeName);
            return att ? att.getValue() : null;
        };
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
        Common.populateName = function (nameField, format) {
            var args = [];
            for (var _i = 2; _i < arguments.length; _i++) {
                args[_i - 2] = arguments[_i];
            }
            if (!Common.getControl(nameField)) {
                return;
            }
            if (args.length === 0) {
                // If there are not any arguments there's nothing to format
                Common.setValue(nameField, format);
                return;
            }
            var hasValue = false;
            // Add Event Handlers
            for (var i = 0; i < args.length; i++) {
                hasValue = hasValue || Common.getValue(args[i]);
                Common.addOnChange(args[i], function () {
                    Common.updateNameHandler(nameField, format, args);
                });
            }
            // Set Name if at least one value is populated and the name field is not
            if (hasValue && !Common.getValue(nameField)) {
                Common.updateNameHandler(nameField, format, args);
            }
        };
        /**
         * Uses the args array to get field's values to populate the field
         *
         * @param nameField The attribute of the field to default
         * @param format The format to use to set the string value of
         * @param args String in the format of "Hello {0}
         * @returns {}
         */
        Common.updateNameHandler = function (nameField, format, args) {
            var i; // check for ending null values to trim string if need be
            // Allows a format of "{0} - {1} - {2}" to be "Foo", rather than "Foo - - ".  
            for (i = 0; i < args.length - 1; i++) {
                args = (args.constructor === Array ? args : [args]);
                if (Common.getValue(args[i])) {
                    continue; // Value not null
                }
                var token = "{" + i + "}";
                var startToken = format.indexOf("{");
                var endToken = format.indexOf("}", startToken);
                var index = void 0;
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
            var formatArgs = args.slice();
            formatArgs.unshift(format);
            for (i = 1; i < formatArgs.length; i++) {
                formatArgs[i] = Common.getDisplayValue(formatArgs[i]);
            }
            Common.setValue(nameField, Common.format.apply(this, formatArgs));
        };
        //#endregion Populate Name
        /**
         * Safe Xrm.Page.getControl(controlName).setDisabled(value)
         * @param controlName Name of the Control to set the disabled of
         * @param disabled True to disable, false to enable
         */
        Common.setDisabled = function (controlName, disabled) {
            if (disabled === void 0) { disabled = true; }
            var ctrl = Common.getControl(controlName);
            if (ctrl) {
                ctrl.setDisabled(disabled);
            }
        };
        /**
         * Safe Xrm.Page.getControl(attributeName).setVisible(visibility)
         *
         * @param controlName The name of the control to set the visibility of
         * @param visibility Optional - Defaults to true
         */
        Common.setVisible = function (controlName, visibility) {
            if (visibility === void 0) { visibility = true; }
            var ctrl = Common.getControl(controlName);
            if (!ctrl) {
                return;
            }
            ctrl.setVisible(visibility);
        };
        return Common;
    })();
    DLaB.Common = Common;
})(DLaB || (DLaB = {}));
