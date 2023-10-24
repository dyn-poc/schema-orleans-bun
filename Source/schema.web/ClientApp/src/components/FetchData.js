import React, { Component } from 'react';
import {Form, Input} from "reactstrap";
// import Ajv from "ajv";
// import $RefParser  from "@apidevtools/json-schema-ref-parser";

export class FetchData extends Component {
  static displayName = FetchData.name;

  constructor(props) {
    super(props);
    this.state = {schema: [], loading: true, apiKey: "apitest", schemas: {}, errors: {}};

  }

  componentDidMount() {
    // this.ajv = new Ajv({loadSchema: this.loadSchema});
    const Ajv2020 = require("ajv/dist/2020")
     this.ajv = new Ajv2020({loadSchema: this.loadSchema, strict:"log", strictSchema:"log"});
    this.ajv.addKeyword("$anchor")
    this.populateJsonSchema();
  }

  static renderJsonSchemaTree(jsonSchema, schemas, errors) {
    console.log(schemas);
    return (
      <ul>
        {Object.keys(jsonSchema).map((key, index) => {
          const value = jsonSchema[key];
          return (
            <li key={index}>
              <span>{key}: </span>
              {typeof value === 'object' ? FetchData.renderJsonSchemaTree(value, schemas, errors) :
                ( <span>{value}</span>)}
            </li>
          );
        })}
      </ul>
    );
  }

  render() {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : FetchData.renderJsonSchemaTree(this.state.schema, this.state.schemas, this.state.errors);

    return (
      <div>
        <h1 id="tabelLabel">Weather forecast</h1>
        <p>This component demonstrates fetching data from the server.</p>
        <Form onSubmit={e => e.preventDefault()}>
          <label>
            API Key:
          </label>
          <Input type="text" name="apiKey" id="apiKey" onChange={(e) => this.setApiKey(e.target.value)}/>
          {contents}
        </Form>
      </div>
    );
  }

  async setApiKey(apiKey) {
    this.setState(prev=> {return {...prev,apiKey: apiKey}});
    await this.populateJsonSchema();
  }

  async populateJsonSchema() {
    const path = `schema/${this.state.apiKey}`;


      console.log(this.state)
      const response = await fetch(path);
      const schema = await response.json();
      console.log(path, {schema});
      this.setState(prev=> {return {...prev, schema: schema, loading: false}});

      // this.ajv.addSchema(data, data.$id);
      // var validate=await  this.ajv.compileAsync(data);
      // console.log(validate);
      // console.log(validate({}));
      // console.log(validate({name: "John", age: 30}));
      // console.log(validate({"profile": {name: "John", age: "30"}}));
      // console.log(this.ajv.schemas);
      // this.setState (prev=> {return {...prev, schema: this.ajv.schemas[data.$id], loading: false, schemas: this.ajv.schemas, errors:this.ajv.errors}});


    }

    // try {
    //   let schema = await $RefParser.dereference(data);
    //   console.log(schema);
    //   console.log(schema.properties.profile.properties.name);
    // }
    // catch(err) {
    //   console.error(err);
    // }


  async loadSchema(uri) {
    console.log(`loadSchema from ${uri})`);

      const response = await fetch(uri);
      if (response.statusCode >= 400) {
        console.log(response);
        throw new Error(`loadSchema from ${uri})`);
      }

      const data = await response.json();

      console.log(`loadSchema from ${uri})`, data);
      return data;


  }

  setDereference(value) {
    console.log("setDereference", value);
    localStorage.setItem("dereference", value);
    this.setState(prev=>{return {...prev, dereference: value}});
    this.populateJsonSchema();

  }
}
