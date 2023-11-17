import {Sample} from "~/samples/Sample";

export const auth:Sample = {
    schema: {
        "$id": "http://localhost:5055/schema/4_DxFqHMTOAJNe9VmFvyO3Uw/auth(bundled)",
        "$defs": {
            "54f89b83dc": {
                "$id": "http://localhost:5055/schema/4_DxFqHMTOAJNe9VmFvyO3Uw/auth",
                "oneOf": [
                    {
                        "$ref": "http://localhost:5055/schema/4_DxFqHMTOAJNe9VmFvyO3Uw/guest"
                    },
                    {
                        "$ref": "http://localhost:5055/schema/4_DxFqHMTOAJNe9VmFvyO3Uw/fido"
                    }
                ]
            },
            "70950b33c0": {
                "$id": "http://localhost:5055/schema/4_DxFqHMTOAJNe9VmFvyO3Uw/guest",
                "properties": {
                    "auth": {
                        "const": "guest"
                    },
                    "user_info": {
                        "properties": {
                            "preferences": {
                                "$ref": "preferences",
                                "properties": {
                                    "terms": true
                                },
                                "additionalProperties": false
                            },
                            "subscriptions": {
                                "$ref": "subscriptions",
                                "properties": {
                                    "newsletter": true
                                },
                                "additionalProperties": false
                            },
                            "data": {
                                "$ref": "profile",
                                "properties": {
                                    "zip": true
                                },
                                "additionalProperties": false
                            },
                            "profile": {
                                "$ref": "data",
                                "properties": {
                                    "email": true,
                                    "firstName": true
                                },
                                "additionalProperties": false
                            }
                        }
                    }
                }
            },
            "c287094322": {
                "$id": "http://localhost:5055/schema/4_DxFqHMTOAJNe9VmFvyO3Uw/fido",
                "properties": {
                    "auth": {
                        "const": "guest"
                    },
                    "user_info": {
                        "properties": {
                            "preferences": {
                                "$ref": "preferences",
                                "properties": {
                                    "terms": true
                                },
                                "additionalProperties": false
                            },
                            "subscriptions": {
                                "$ref": "subscriptions",
                                "properties": {
                                    "newsletter": true
                                },
                                "additionalProperties": false
                            },
                            "data": {
                                "$ref": "profile",
                                "properties": {
                                    "zip": true
                                },
                                "additionalProperties": false
                            },
                            "profile": {
                                "$ref": "data",
                                "properties": {
                                    "email": true,
                                    "firstName": true
                                },
                                "additionalProperties": false
                            }
                        }
                    }
                }
            },
            "9d65a57180": {
                "$id": "preferences",
                "title": "preferences",
                "description": "preferences",
            },
            "7ab18c40bd": {
                "$id": "subscriptions",
                "title": "subscriptions",
                "description": "subscriptions",
                "$anchor": "subscriptions",
                "additionalProperties": false,
                "properties": {
                    "news": {
                        "title": "news",
                        "description": "news",
                        "type": [
                            "object",
                            "null"
                        ],
                        "additionalProperties": false,
                        "x-field-write-access": "serverOnly"
                    }
                }
            },
            "e297424b29": {
                "$id": "http://localhost:5055/schema/4_DxFqHMTOAJNe9VmFvyO3Uw/profile",
                "title": "profile",
                "description": "profile",
                "$anchor": "profile",
                "additionalProperties": false,
                "properties": {
                    "email": {
                        "title": "email",
                        "description": "email",
                        "$anchor": "email",
                        "type": [
                            "string",
                            "null"
                        ],
                        "x-field-required-for-login": true,
                        "x-field-write-access": "clientModify",
                        "x-field-encrypt": "AES"
                    },
                    "birthYear": {
                        "title": "birthYear",
                        "description": "birthYear",
                        "$anchor": "birthYear",
                        "type": [
                            "number",
                            "null"
                        ],
                        "x-field-required-for-login": false,
                        "x-field-write-access": "clientModify"
                    },
                    "firstName": {
                        "title": "firstName",
                        "description": "firstName",
                        "$anchor": "firstName",
                        "type": [
                            "string",
                            "null"
                        ],
                        "x-field-required-for-login": false,
                        "x-field-write-access": "clientModify",
                        "x-field-encrypt": "AES"
                    },
                    "lastName": {
                        "title": "lastName",
                        "description": "lastName",
                        "$anchor": "lastName",
                        "type": [
                            "string",
                            "null"
                        ],
                        "x-field-required-for-login": false,
                        "x-field-write-access": "clientModify",
                        "x-field-encrypt": "AES"
                    },
                    "zip": {
                        "title": "zip",
                        "description": "zip",
                        "$anchor": "zip",
                        "type": [
                            "string",
                            "null"
                        ],
                        "x-field-required-for-login": false,
                        "x-field-write-access": "clientModify",
                        "x-field-encrypt": "AES"
                    },
                    "country": {
                        "title": "country",
                        "description": "country",
                        "$anchor": "country",
                        "type": [
                            "string",
                            "null"
                        ],
                        "x-field-required-for-login": false,
                        "x-field-write-access": "clientModify",
                        "x-field-encrypt": "AES"
                    },
                    "education": {
                        "title": "education",
                        "description": "education",
                        "$anchor": "education",
                        "additionalProperties": false,
                        "properties": {
                            "degree": {
                                "title": "degree",
                                "description": "degree",
                                "$anchor": "degree",
                                "type": [
                                    "string",
                                    "null"
                                ],
                                "x-field-required-for-login": false,
                                "x-field-write-access": "clientModify"
                            }
                        }
                    }
                }
            },
            "42f7bcbc1d": {
                "$id": "data",
                "title": "data",
                "description": "data",
                "$anchor": "data",
                "additionalProperties": false,
                "properties": {
                    "terms": {
                        "title": "terms",
                        "description": "terms",
                        "$anchor": "terms",
                        "type": [
                            "boolean",
                            "null"
                        ],
                        "x-field-required-for-login": false,
                        "x-field-write-access": "clientModify"
                    },
                    "subscribe": {
                        "title": "subscribe",
                        "description": "subscribe",
                        "$anchor": "subscribe",
                        "type": [
                            "boolean",
                            "null"
                        ],
                        "x-field-required-for-login": false,
                        "x-field-write-access": "clientModify"
                    }
                }
            },
            "d1460501cb": {
                "$id": "preferences",
                "title": "preferences",
                "description": "preferences",
                "$anchor": "preferences"
            },
            "20bd040314": {
                "$id": "subscriptions",
                "title": "subscriptions",
                "description": "subscriptions",
                "$anchor": "subscriptions",
                "additionalProperties": false,
                "properties": {
                    "news": {
                        "title": "news",
                        "description": "news",
                        "$anchor": "news",
                        "type": [
                            "object",
                            "null"
                        ],
                        "additionalProperties": false,
                        "x-field-write-access": "serverOnly"
                    }
                }
            },
            "88514d4e44": {
                "$id": "profile",
                "title": "profile",
                "description": "profile",
                "$anchor": "profile",
                "additionalProperties": false,
                "properties": {
                    "email": {
                        "title": "email",
                        "description": "email",
                        "$anchor": "email",
                        "type": [
                            "string",
                            "null"
                        ],
                        "x-field-required-for-login": true,
                        "x-field-write-access": "clientModify",
                        "x-field-encrypt": "AES"
                    },
                    "birthYear": {
                        "title": "birthYear",
                        "description": "birthYear",
                        "$anchor": "birthYear",
                        "type": [
                            "number",
                            "null"
                        ],
                        "x-field-required-for-login": false,
                        "x-field-write-access": "clientModify"
                    },
                    "firstName": {
                        "title": "firstName",
                        "description": "firstName",
                        "$anchor": "firstName",
                        "type": [
                            "string",
                            "null"
                        ],
                        "x-field-required-for-login": false,
                        "x-field-write-access": "clientModify",
                        "x-field-encrypt": "AES"
                    },
                    "lastName": {
                        "title": "lastName",
                        "description": "lastName",
                        "$anchor": "lastName",
                        "type": [
                            "string",
                            "null"
                        ],
                        "x-field-required-for-login": false,
                        "x-field-write-access": "clientModify",
                        "x-field-encrypt": "AES"
                    },
                    "zip": {
                        "title": "zip",
                        "description": "zip",
                        "$anchor": "zip",
                        "type": [
                            "string",
                            "null"
                        ],
                        "x-field-required-for-login": false,
                        "x-field-write-access": "clientModify",
                        "x-field-encrypt": "AES"
                    },
                    "country": {
                        "title": "country",
                        "description": "country",
                        "$anchor": "country",
                        "type": [
                            "string",
                            "null"
                        ],
                        "x-field-required-for-login": false,
                        "x-field-write-access": "clientModify",
                        "x-field-encrypt": "AES"
                    },
                    "education": {
                        "title": "education",
                        "description": "education",
                        "$anchor": "education",
                        "additionalProperties": false,
                        "properties": {
                            "degree": {
                                "title": "degree",
                                "description": "degree",
                                "$anchor": "degree",
                                "type": [
                                    "string",
                                    "null"
                                ],
                                "x-field-required-for-login": false,
                                "x-field-write-access": "clientModify"
                            }
                        }
                    }
                }
            },
            "531a19f2da": {
                "$id": "data",
                "title": "data",
                "description": "data",
                "$anchor": "data",
                "additionalProperties": false,
                "properties": {
                    "terms": {
                        "title": "terms",
                        "description": "terms",
                        "$anchor": "terms",
                        "type": [
                            "boolean",
                            "null"
                        ],
                        "x-field-required-for-login": false,
                        "x-field-write-access": "clientModify"
                    },
                    "subscribe": {
                        "title": "subscribe",
                        "description": "subscribe",
                        "$anchor": "subscribe",
                        "type": [
                            "boolean",
                            "null"
                        ],
                        "x-field-required-for-login": false,
                        "x-field-write-access": "clientModify"
                    }
                }
            }
        },
        "$ref": "http://localhost:5055/schema/4_DxFqHMTOAJNe9VmFvyO3Uw/auth"
    }
};

export  default auth;
