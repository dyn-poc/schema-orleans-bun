import {Sample} from "~/samples/Sample";

const channel_dependencies: Sample = {

    schema: {
        "type": "object",
        "properties": {
            "communication": {
                "title": "Communication",
                "properties": {
                    "sms_newsletter": {
                        "title": "SMS Newsletter",
                        "allOf": [
                            {"$ref": "sms_channel"},
                            {"$ref": "communication_entry"}
                        ]
                    },
                    "email_newsletter": {
                        "title": "Email Newsletter",
                        "allOf": [
                            {"$ref": "email_channel"},
                            {"$ref": "communication_entry"},
                        ]
                    },
                    "aliexpress_orders": {
                        "title": "Aliexpress Orders",
                        "$ref": "aliexpress_orders"
                    }
                }
            },
            "data": {
                "title": "Data",
                "type": "object",
                "properties": {
                    "shipping_address": {
                        "type": "string"
                    }
                }
            },
            "phonenumber": {
                "type": "string",
                "pattern": "^\\+?[0-9]{10,14}$"
            },
            "profile": {
                "properties": {

                    "email": {
                        "type": "string",
                        "format": "email"
                    }
                }
            },
        },

        "allOf": [
            {
                "if": {"properties": {"communication": {"properties": {"email_newsletter": {"$ref": "opt-in"}}}}},
                "then": {
                    "properties": {
                        "profile": {
                            "required": ["email"]
                        }
                    }
                }, "else": true
            },
            {
                "if": {"properties": {"communication": {"properties": {"sms_newsletter": {"$ref": "opt-in"}}}}},
                "then": {"required": ["phonenumber"]},
                "else": true
            },
            {
                "if": {"properties": {"communication": {"properties": {"aliexpress_orders": {"$ref": "opt-in"}}}}},
                "then": {"required": ["data"], "properties": {"data": {"required": ["shipping_address"]}}},
                "else": true
            }
        ],
        "$defs": {
            "communication_entry": {
                "$id": "communication_entry",
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
                        "enum": [
                            "email",
                            "sms",
                            "shipping"
                        ]
                    },
                }
            },
            "aliexpress_orders": {
                "$id": "aliexpress_orders",
                "type": "object",
                "allOf": [
                    {"$ref": "shipping_channel"},
                    {"$ref": "communication_entry"}
                ]
            },
            "shipping_channel": {
                "$id": "shipping_channel",
                "$ref": "#/$defs/channel",

                "$defs": {
                    "channel": {
                        "properties": {
                            "channel": {
                                "enum": ["shipping"],
                                "default": "shipping"
                            }
                        }
                    }
                }
            },
            "sms_channel": {
                "$id": "sms_channel",
                "type": "object",
                "properties": {
                    "channel": {
                        "enum": ["sms"],
                        "default": "sms"
                    }
                }
            },
            "email_channel": {
                "$id": "email_channel",
                "type": "object",
                "properties": {
                    "channel": {
                        "enum": ["email"],
                        "default": "email"
                    }
                }
            },
            "opt-in": {
                "$id": "opt-in",
                "type": "object",
                "properties": {
                    "status": {
                        "enum": ["opt-in"]
                    }
                }
            },
            "opt-out": {
                "$id": "opt-out",
                "type": "object",
                "properties": {
                    "status": {
                        "enum": ["opt-out"]
                    }
                }
            },
            "notice": {
                "$id": "notice",
                "type": "object",
                "properties": {
                    "status": {
                        "enum": ["notice"]
                    }
                }
            }
        },
        "$id": "source-schema"
    },
    uiSchema: {
        "communication": {
            "email_newsletter": {
                "channel": {
                    "ui:readonly": true
                },
                "status": {
                    "ui:widget": "select"
                },
                "message": {
                    "ui:readonly": true,
                    "ui:widget": "hidden"

                },
                "duration": {
                    "ui:readonly": true,
                    "ui:widget": "hidden"
                }
            },

            "sms_newsletter": {
                "channel": {
                    "ui:readonly": true
                },
                "status": {
                    "ui:widget": "select"
                }
            },
            "aliexpress_orders": {
                "channel": {
                    "ui:readonly": true
                }
            }
        }

    },
    formData: {
        "communication": {
            "email_newsletter": {
                "status": "notice"
            },
            "sms_newsletter": {
                "status": "notice"
            },
            "aliexpress_orders": {
                "status": "notice"
            }
        }

    }
}

export default channel_dependencies;
