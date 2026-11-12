FROM mcr.microsoft.com/dotnet/runtime-deps:11.0-noble

ARG PUBLISH_DIR=artifacts/publish/ApiService
ARG INSTALL_FONTS=false
ARG CJK_FONT_PACKAGE=fonts-wqy-zenhei

RUN if [ "$INSTALL_FONTS" = "true" ]; then \
        apt-get update && apt-get install --yes --no-install-recommends \
            fontconfig \
            fonts-dejavu-core \
            fonts-noto-color-emoji \
            ${CJK_FONT_PACKAGE} && \
        rm -rf /var/lib/apt/lists/* && \
        fc-cache -f; \
    fi

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LANG=C.UTF-8

WORKDIR /app
COPY ${PUBLISH_DIR}/ ./

EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["./ApiService"]
