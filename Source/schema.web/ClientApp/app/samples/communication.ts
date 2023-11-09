import {Sample} from "~/samples/Sample";

const communication: Sample = {

  schema: {
    "$schema": "https://json-schema.org/draft/2020-12/schema",
    "type": "object",

    "required": [
      "status"
    ],
    "oneOf": [
      {
        "$ref": "opt-in",
        "title": "Opt-in"
      },
      {
        "$ref": "opt-out",
        "title": "Opt-out"
      },
      {
        "$ref": "notice",
        "title": "Notice"
      }
    ],
    "$defs": {
      "opt-in": {
        "$id": "opt-in",
        "type": "object",
        "properties": {
          "status": {
            "const": "opt-in",
            "default": "opt-in"
          },
          "interests": {
            "type": "array",
            "items": {
              "type": "string",
              "enum": [
                "pets",
                "baby",
                "boms"
              ]
            },
            uniqueItems: true,
            "minItems": 1
          }
        },
        "required": [
          "interests"
        ],
        "additionalProperties": false
      },
      "opt-out": {
        "$id": "opt-out",
        "type": "object",
        "properties": {
          "status": {
            "const": "opt-out",
            "default": "opt-out"
          },
          "reason": {
            "type": "string"
          }
        },
        "required": [
          "reason"
        ]
      },
      "notice": {
        "$id": "notice",
        "type": "object",
        "properties": {
          "status": {
            "const": "notice",
            "default": "notice"
          },
          "message": {
            "type": "string"
          },
          "duration": {
            "type": "integer"
          }
        },
        "required": [
          "message",
          "duration"
        ]
      }
    },
    "$id": "source-schema"
  },
  uiSchema: {
    // status: {
    //   'ui:widget': 'radio',
    //   'ui:options': {
    //     optIn: 'Opt-in',
    //     optOut: 'Opt-out',
    //     notice: 'Notice'
    //   }
    // },
    "status": {
      'ui:widget': 'hidden'
    },
    interests: {
      'ui:widget': 'checkboxes'

    },
    reason: {
      'ui:widget': 'textarea'
    },
    message: {
      'ui:widget': 'textarea'
    },
    duration: {
      'ui:widget': 'updown'
    }
  },
  formData: {
    status: 'opt-in',
    interests: ['pets']
  }
};

export default communication;
