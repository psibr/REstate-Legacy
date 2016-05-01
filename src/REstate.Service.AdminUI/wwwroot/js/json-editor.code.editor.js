JSONEditor.defaults.editors.code = JSONEditor.AbstractEditor.extend({
    register: function () {
        this._super();
        if (!this.input) return;
        this.input.setAttribute('name', this.formname);
    },
    unregister: function () {
        this._super();
        if (!this.input) return;
        this.input.removeAttribute('name');
    },
    setValue: function (value, initial, from_template) {
        var self = this;

        if (this.template && !from_template) {
            return;
        }

        if (value === null || typeof value === 'undefined') value = "";
        else if (typeof value === "object") value = JSON.stringify(value);
        else if (typeof value !== "string") value = "" + value;

        if (value === this.serialized) return;

        this.input.value = value;

        if(this.ace_editor) {
            this.ace_editor.setValue(this.input.value);
        }


        var changed = from_template || this.getValue() !== value;

        this.refreshValue();

        if (initial) this.is_dirty = false;
        else if (this.jsoneditor.options.show_errors === "change") this.is_dirty = true;

        if (this.adjust_height) this.adjust_height(this.input);

        // Bubble this setValue to parents if the value changed
        this.onChange(changed);
    },
    getNumColumns: function () {
        var min = Math.ceil(Math.max(this.getTitle().length, this.schema.maxLength || 0, this.schema.minLength || 0) / 5);
        var num;

        if (this.input_type === 'textarea') num = 6;
        else if (['text', 'email'].indexOf(this.input_type) >= 0) num = 4;
        else num = 2;

        return Math.min(12, Math.max(min, num));
    },
    build: function () {
        var self = this, i;
        if (!this.options.compact) this.header = this.label = this.theme.getFormInputLabel(this.getTitle());
        if (this.schema.description) this.description = this.theme.getFormInputDescription(this.schema.description);

        this.format = this.schema.format;
        if (!this.format && this.schema.media && this.schema.media.type) {
            this.format = this.schema.media.type.replace(/(^(application|text)\/(x-)?(script\.)?)|(-source$)/g, '');
        }
        if (!this.format && this.options.default_format) {
            this.format = this.options.default_format;
        }

        this.input_type = this.options.format || "text";
        this.source_code = true;

        this.input = this.theme.getTextareaInput();


        // minLength, maxLength, and pattern
        if (typeof this.schema.maxLength !== "undefined") this.input.setAttribute('maxlength', this.schema.maxLength);
        if (typeof this.schema.pattern !== "undefined") this.input.setAttribute('pattern', this.schema.pattern);
        else if (typeof this.schema.minLength !== "undefined") this.input.setAttribute('pattern', '.{' + this.schema.minLength + ',}');

        if (this.options.compact) {
            this.container.className += ' compact';
        }
        else {
            if (this.options.input_width) this.input.style.width = this.options.input_width;
        }

        if (this.schema.readOnly || this.schema.readonly || this.schema.template) {
            this.always_disabled = true;
            this.input.disabled = true;
        }

        this.input
            .addEventListener('change', function (e) {
                e.preventDefault();
                e.stopPropagation();

                // Don't allow changing if this field is a template
                if (self.schema.template) {
                    this.value = self.value;
                    return;
                }

                var val = this.value;

                // sanitize value
                var sanitized = self.sanitize(val);
                if (val !== sanitized) {
                    this.value = sanitized;
                }

                self.is_dirty = true;

                self.refreshValue();
                self.onChange(true);
            });

        if (this.options.input_height) this.input.style.height = this.options.input_height;
        if (this.options.expand_height) {
            this.adjust_height = function (el) {
                if (!el) return;
                var i, ch = el.offsetHeight;
                // Input too short
                if (el.offsetHeight < el.scrollHeight) {
                    i = 0;
                    while (el.offsetHeight < el.scrollHeight + 3) {
                        if (i > 100) break;
                        i++;
                        ch++;
                        el.style.height = ch + 'px';
                    }
                }
                else {
                    i = 0;
                    while (el.offsetHeight >= el.scrollHeight + 3) {
                        if (i > 100) break;
                        i++;
                        ch--;
                        el.style.height = ch + 'px';
                    }
                    el.style.height = (ch + 1) + 'px';
                }
            };

            this.input.addEventListener('keyup', function (e) {
                self.adjust_height(this);
            });
            this.input.addEventListener('change', function (e) {
                self.adjust_height(this);
            });
            this.adjust_height();
        }

        if (this.format) this.input.setAttribute('data-schemaformat', this.format);

        this.control = this.theme.getFormControl(this.label, this.input, this.description);
        this.container.appendChild(this.control);

        // Any special formatting that needs to happen after the input is added to the dom
        window.requestAnimationFrame(function () {
            // Skip in case the input is only a temporary editor,
            // otherwise, in the case of an ace_editor creation,
            // it will generate an error trying to append it to the missing parentNode
            if (self.input.parentNode) self.afterInputReady();
            if (self.adjust_height) self.adjust_height(self.input);
        });

        // Compile and store the template
        if (this.schema.template) {
            this.template = this.jsoneditor.compileTemplate(this.schema.template, this.template_engine);
            this.refreshValue();
        }
        else {
            this.refreshValue();
        }
    },
    enable: function () {
        if (!this.always_disabled) {
            this.input.disabled = false;

        }
        this._super();
    },
    disable: function () {
        this.input.disabled = true;

        this._super();
    },
    afterInputReady: function () {
        var self = this, options;

        // Code editor
        if (this.source_code) {

            var mode = this.input_type;
            // aliases for c/cpp
            if (mode === 'cpp' || mode === 'c++' || mode === 'c') {
                mode = 'c_cpp';
            }

            this.code_container = document.createElement('div');
            
            this.dropdownLabel = document.createElement('label');
            this.dropdownLabel.className = "control-label";
            this.dropdownLabel.innerText = "Syntax";
              
            this.syntaxDropdown = document.createElement('select');
            this.syntaxDropdown.className = "form-control";
            
            this.textOption = document.createElement('option');
            this.textOption.value = "text";
            this.textOption.innerText = "Text";
            
            this.jsonOption = document.createElement('option');
            this.jsonOption.value = "json";
            this.jsonOption.innerText = "JSON";
            
            this.sqlOption = document.createElement('option');
            this.sqlOption.value = "sql";
            this.sqlOption.innerText = "SQL";
            
            this.csharpOption = document.createElement('option');
            this.csharpOption.value = "csharp";
            this.csharpOption.innerText = "C#";
            
            this.sqlServerOption = document.createElement('option');
            this.sqlServerOption.value = "sqlserver";
            this.sqlServerOption.innerText = "SQL Server";
            
            this.yamlOption = document.createElement('option');
            this.yamlOption.value = "yaml";
            this.yamlOption.innerText = "YAML";
            
            this.jsOption = document.createElement('option');
            this.jsOption.value = "javascript";
            this.jsOption.innerText = "Javascript";
            
            this.tsOption = document.createElement('option');
            this.tsOption.value = "typescript";
            this.tsOption.innerText = "TypeScript";
            
            this.htmlOption = document.createElement('option');
            this.htmlOption.value = "html";
            this.htmlOption.innerText = "HTML";
            
            this.syntaxDropdown.appendChild(this.textOption);
            this.syntaxDropdown.appendChild(this.jsonOption);
            this.syntaxDropdown.appendChild(this.sqlOption);
            this.syntaxDropdown.appendChild(this.csharpOption);
            this.syntaxDropdown.appendChild(this.sqlServerOption);
            this.syntaxDropdown.appendChild(this.yamlOption);
            this.syntaxDropdown.appendChild(this.jsOption);
            this.syntaxDropdown.appendChild(this.tsOption);
            this.syntaxDropdown.appendChild(this.htmlOption);
            
            this.syntaxDropdown.value = mode;
            
            this.ace_container = document.createElement('div');
            this.ace_container.style.width = '99%';
            this.ace_container.style.position = 'relative';
            this.ace_container.style.height = '400px';
            
            this.code_container.appendChild(this.dropdownLabel);
            this.code_container.appendChild(this.syntaxDropdown);
            this.code_container.appendChild(this.ace_container);
            
            this.input.parentNode.insertBefore(this.code_container, this.input);
            this.input.style.display = 'none';
            
            
            this.ace_editor = window.ace.edit(this.ace_container);

            this.ace_editor.setValue(this.getValue());

            // The theme
            if (JSONEditor.plugins.ace.theme) this.ace_editor.setTheme('ace/theme/' + JSONEditor.plugins.ace.theme);
            // The mode
            mode = window.ace.require("ace/mode/" + mode);
            if (mode) this.ace_editor.getSession().setMode(new mode.Mode());

            // Listen for changes
            this.ace_editor.on('change', function () {
                var val = self.ace_editor.getValue();
                self.input.value = val;
                self.refreshValue();
                self.is_dirty = true;
                self.onChange(true);
            });
            
            var handler = function(evt) 
            {
                var newMode = window.ace.require("ace/mode/" + $(this.syntaxDropdown).val());
                this.ace_editor.getSession().setMode(new newMode.Mode());
            }.bind(this);
            
            $(this.syntaxDropdown).on('change', handler );
        }


        self.theme.afterInputReady(self.input);
    },
    refreshValue: function () {
        this.value = this.input.value;
        if (typeof this.value !== "string") this.value = '';
        this.serialized = this.value;
    },
    destroy: function () {

        this.ace_editor.destroy();


        this.template = null;
        if (this.input && this.input.parentNode) this.input.parentNode.removeChild(this.input);
        if (this.label && this.label.parentNode) this.label.parentNode.removeChild(this.label);
        if (this.description && this.description.parentNode) this.description.parentNode.removeChild(this.description);

        this._super();
    },
    /**
     * This is overridden in derivative editors
     */
    sanitize: function (value) {
        return value;
    },
    /**
     * Re-calculates the value if needed
     */
    onWatchedFieldChange: function () {
        var self = this, vars, j;

        // If this editor needs to be rendered by a macro template
        if (this.template) {
            vars = this.getWatchedFieldValues();
            this.setValue(this.template(vars), false, true);
        }

        this._super();
    },
    showValidationErrors: function (errors) {
        var self = this;

        if (this.jsoneditor.options.show_errors === "always") { }
        else if (!this.is_dirty && this.previous_error_setting === this.jsoneditor.options.show_errors) return;

        this.previous_error_setting = this.jsoneditor.options.show_errors;

        var messages = [];
        $.each(errors, function (i, error) {
            if (error.path === self.path) {
                messages.push(error.message);
            }
        });

        if (messages.length) {
            this.theme.addInputError(this.input, messages.join('. ') + '.');
        }
        else {
            this.theme.removeInputError(this.input);
        }
    }
});
