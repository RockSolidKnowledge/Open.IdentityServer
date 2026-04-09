(Prism.languages.alfa = Prism.languages.extend("clike", {
    
    declaration: { 
        pattern: /\b(?<=.*\s)(.*)(?=\s?{)/, 
        greedy: true 
    },
    keyword: {
        pattern: /\b(?:advice|obligation|apply|attribute|category|deny|id|let|import|namespace|on|permit|policy|policyset|rule|function|target|type)\b/,
        greedy: false
    },
    "keyword-alt": {
        pattern: /\b(?:clause|and|or|condition|denyOverrides|permitOverrides|firstApplicable|onlyOneApplicable|orderedDenyOverrides|orderedPermitOverrides|denyUnlessPermit|permitUnlessDeny|onPermitApplySecond)\b/,
        greedy: false
    },
    string: [
        { pattern: /@("|')(?:\1\1|\\[\s\S]|(?!\1)[^\\])*\1/, greedy: false },
        { pattern: /("|')(?:\\.|(?!\1)[^\\\r\n])*?\1/, greedy: false },
    ],
    number: {
        pattern: /\b0x[\da-f]+\b|(?:\b\d+\.?\d*|\B\.\d+)f?/i,
        greedy: false
    },
    type: {
        pattern: /\b(?:integer|double|boolean|date|time|dateTime|string|duration)\b/,
        greedy: false
    },
    property: {
        pattern: /(\s?=)\s?[a-z][a-zA-Z0-9_-]+/, inside: { keyword: /\=/ },
        greedy: true
    },
    identifier: {
        pattern: /\b[_A-Za-z][_a-zA-Z0-9]*(?:\.[_A-Za-z][_a-zA-Z0-9])*/, inside: { punctuation: /\./ },
        greedy: false
    },
}));