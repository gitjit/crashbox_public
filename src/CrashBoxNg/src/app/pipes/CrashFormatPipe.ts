import {Pipe,PipeTransform} from '@angular/core';

@Pipe({
    name:'formatCrash'
})

export class CrashFormatPipe implements PipeTransform{

    transform(value:string, arg:string):string{
        if (arg == 'at') {
            value = value.replace('at', '');
            value = value.substring(0, 50);
            value = value + '...';
            return value;
          }
      
          if (arg == "id") {
            value = value.substr(0, 4);
            return value;
          }
      
          if (arg == "30c") {
            if(value.length < 30) return value.trim();
            value = value.substr(0, 30);
            value = value + '...';
            value = value.trim();
            return value;
          }
    }
}