export function get(key){ try{ return window.localStorage.getItem(key); }catch{ return null; } }
export function set(key, value){ try{ window.localStorage.setItem(key, value); }catch{} }
export function sget(key){ try{ return window.sessionStorage.getItem(key); }catch{ return null; } }
export function sset(key, value){ try{ window.sessionStorage.setItem(key, value); }catch{} }


