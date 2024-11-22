"use strict";

const key = [2815074099, 1725469378, 4039046167, 874293617, 3063605751, 3133984764, 4097598161, 3620741625];
sjcl.beware["CBC mode is dangerous because it doesn't protect message integrity."]();

window.shelter = { 
    readFileAsBase64: async function(fileInput) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onerror = () => {
                reader.abort();
                reject(new Error("Error reading file."));
            };
            reader.onload = () => {
                resolve(reader.result);
            };
            reader.readAsText(fileInput.files[0]);
        });
    },

    decryptString: async function (base64String) {        
        return new Promise((resolve, reject) => {            
            const iv = sjcl.codec.hex.toBits("7475383967656A693334307438397532");
            const cipherBits = sjcl.codec.base64.toBits(base64String);
            const prp = new sjcl.cipher.aes(key);
            const plainBits = sjcl.mode.cbc.decrypt(prp, cipherBits, iv);
            const jsonStr = sjcl.codec.utf8String.fromBits(plainBits);
            try {
                resolve(jsonStr);
            } catch (e) {
                reject(new Error("Error Decrypting String"));
            }
        });
    },

    downloadFile: async function(filename, content, contentType) {
        const blob = new Blob([content], { type: contentType });
        const url = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
    }
}