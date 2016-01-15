

http://nginx.org/en/docs/syslog.html


Logging to syslog

The error_log and access_log directives support logging to syslog. The following parameters configure logging to syslog:

server=address
    Defines the address of a syslog server. The address can be specified as a domain name or IP address, with an optional port, or as a UNIX-domain socket path specified after the “unix:” prefix. If port is not specified, the UDP port 514 is used. If a domain name resolves to several IP addresses, the first resolved address is used. 
facility=string
    Sets facility of syslog messages, as defined in RFC 3164. Facility can be one of “kern”, “user”, “mail”, “daemon”, “auth”, “intern”, “lpr”, “news”, “uucp”, “clock”, “authpriv”, “ftp”, “ntp”, “audit”, “alert”, “cron”, “local0”..“local7”. Default is “local7”. 
severity=string
    Sets severity of syslog messages for access_log, as defined in RFC 3164. Possible values are the same as for the second parameter (level) of the error_log directive. Default is “info”.

        Severity of error messages is determined by nginx, thus the parameter is ignored in the error_log directive. 

tag=string
    Sets the tag of syslog messages. Default is “nginx”. 
nohostname
    Disables adding the “hostname” field into the syslog message header (1.9.7). 

Example syslog configuration:

    error_log syslog:server=192.168.1.1 debug;

    access_log syslog:server=unix:/var/log/nginx.sock,nohostname;
    access_log syslog:server=[2001:db8::1]:12345,facility=local7,tag=nginx,severity=info combined;

    Logging to syslog is available since version 1.7.1. As part of our commercial subscription logging to syslog is available since version 1.5.3. 


-------------------------------------------



Syslog-daemon-for-Windows-Eventlog
http://www.codeproject.com/Articles/18086/Syslog-daemon-for-Windows-Eventlog








https://github.com/neutmute/loggly-csharp

http://www.codeproject.com/Tips/441233/Multithreaded-Customizable-SysLog-Server-Csharp

https://code.google.com/p/aonawaresyslog/

http://michael.chanceyjr.com/useful-code/syslogd-class-for-sending-and-receiving-syslogd-events/


http://blog.graffen.dk/post/logging-messages-to-a-syslog-server-using-nlog

http://en.pudn.com/downloads556/sourcecode/windows/csharp/detail2292252_en.html