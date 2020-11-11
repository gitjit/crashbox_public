export interface ICrash {
    id: string;
    pk: string;
    _ts: number;
    app: string;
    version: string;
    os: string;
    region: string;
    language: string;
    method: string;
    mhash: number;
    log: string;
    extype: string;
    stack: string;
  }
  