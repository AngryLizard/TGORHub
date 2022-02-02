
export async function hash(message: string): Promise<string> {
    const msgBuffer = new TextEncoder().encode(message);

    // hash the message
    const hashBuffer = await crypto.subtle.digest('SHA-256', msgBuffer);

    // convert ArrayBuffer to Array
    const hashArray = Array.from(new Uint8Array(hashBuffer));

    // convert bytes to hex string                  
    const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
    return hashHex;
}

export function delay(time: number) {
    return new Promise(resolve => setTimeout(resolve, time));
}

export function setValidity(tag: string, valid: boolean) {
    $(tag).toggleClass("is-invalid", !valid);
    $(tag).toggleClass("is-valid", valid);
}