import React, { Component } from 'react';
import * as monaco from 'monaco-editor';

class ReactMonacoEditor extends Component {
    private editorRef: React.RefObject<unknown>;
    constructor(props: {} | Readonly<{}>) {
        super(props);
        this.editorRef = React.createRef();
        this.editor = null;
        this.form = null;
    }

    componentDidMount() {
        this.initMonaco();
        this.attachFormSubmitListener();
    }

    componentDidUpdate(prevProps) {
        if (this.props.value !== prevProps.value) {
            this.editor.setValue(this.props.value);
        }

        if (this.props.language !== prevProps.language) {
            const currentModel = this.editor.getModel();
            if (currentModel) {
                currentModel.dispose();
            }
            this.editor.setModel(monaco.editor.createModel(this.props.value, this.props.language));
        }
    }

    componentWillUnmount() {
        if (this.editor) {
            this.editor.dispose();
        }
        this.detachFormSubmitListener();
    }

    initMonaco() {
        this.editor = monaco.editor.create(this.editorRef.current, {
            theme: 'vs-dark',
            model: monaco.editor.createModel(this.props.value, this.props.language),
            wordWrap: 'on',
            automaticLayout: true,
            minimap: {
                enabled: false
            },
            scrollbar: {
                vertical: 'auto'
            }
        });
    }

    attachFormSubmitListener() {
        // Find the nearest parent form
        this.form = this.editorRef.current.closest('form');
        if (this.form) {
            this.form.addEventListener('submit', this.handleFormSubmit);
        }
    }

    detachFormSubmitListener() {
        if (this.form) {
            this.form.removeEventListener('submit', this.handleFormSubmit);
        }
    }

    handleFormSubmit = (event) => {
        // Append the value of the editor to the FormData
        const formData = new FormData(this.form);
        formData.append(this.props.name, this.editor.getValue());
        // Optional: If you want to prevent default form submission
        // event.preventDefault();
    }

    render() {
        return (
            <div ref={this.editorRef} style={{ height: '100%', width: '100%' }} />
        );
    }
}

export default ReactMonacoEditor;
