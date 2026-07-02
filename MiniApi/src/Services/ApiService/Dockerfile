FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine-extra

ARG PUBLISH_DIR=artifacts/publish/ApiService
ARG INSTALL_FONTS=false
ARG CJK_FONT_PACKAGE=font-wqy-zenhei

RUN if [ "$INSTALL_FONTS" = "true" ]; then \
        apk add --no-cache \
            fontconfig \
            font-dejavu \
            font-noto-emoji \
            ${CJK_FONT_PACKAGE} && fc-cache -f; \
    fi

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LANG=C.UTF-8

WORKDIR /app
COPY ${PUBLISH_DIR}/ ./

EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["./ApiService"] 