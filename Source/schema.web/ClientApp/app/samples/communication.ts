import {Sample} from "~/samples/Sample";

const communication: Sample = {

  schema: {
    "$id": "communication-entry",
    "required": [
      "status"
    ],
    "properties": {
      "status": {
        "type": "string",
        "enum": ["opt-in", "opt-out", "notice"],
        "default": "notice"
      },
      "channel": {
        "type": "string",
        "readOnly": true,
        "default": "email",
        "enum": [
          "email",
          "sms",
          "shipping"
        ]
      },
    },
    "dependencies": {
      "status": {
        "oneOf": [
          {
            "$ref": "opt-in"
          },
          {
            "$ref": "opt-out"
          },
          {
            "$ref": "notice"
          }
        ]
      }
    },
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
    }
    },
  uiSchema: {

    "status": {
      'ui:widget': 'select',
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
