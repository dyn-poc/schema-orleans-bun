
import React from "react";
import Editor from '@monaco-editor/react';
 // import * as monaco from 'monaco-editor';
type JsonProps = {
  src: any
};

export default function MonacoJson({src}: JsonProps) {

  function getOptions()
  {
    return {
      json: true,
      readOnly: true,
      jsonDefaults: {
        schemas: [
          {
            uri: "http://myserver/bar-schema.json", // id of the second schema
            schema: {
              type: "object",
              properties: {
                q1: {
                  enum: ["x1", "x2"],
                },
              },
            },
          },
        ]
      }
    };
  }

  return (
      <Editor
        height="90vh"
        defaultLanguage="json"
        options={getOptions()}
        defaultValue={JSON.stringify(src, undefined, 4)}
      />
  )
}

