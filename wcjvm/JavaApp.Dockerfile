FROM wcjvm

WORKDIR /

COPY App.class /

CMD [ "-XX:+UseG1GC", "App" ]
