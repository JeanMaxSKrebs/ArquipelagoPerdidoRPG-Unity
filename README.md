# Arquipelago Perdido RPG - Unity

Projeto do jogo **Arquipélago Perdido RPG** desenvolvido em **Unity**.

## Visão geral

Este repositório contém a nova versão do projeto em Unity, organizada para evolução contínua do jogo com foco em estrutura limpa, versionamento correto e separação entre assets do projeto e pacotes externos.

## Estrutura principal

- `Assets/` → arquivos do projeto, cenas, scripts, prefabs, materiais e pacotes utilizados
- `Packages/` → dependências do Unity
- `ProjectSettings/` → configurações do projeto
- `.gitignore` → exclusões importantes para Unity e arquivos locais

## Organização em Assets

A pasta principal de desenvolvimento do jogo é:

- `Assets/_Project/`

Dentro dela, a ideia é centralizar:
- `Scenes/`
- `Scripts/`
- `Prefabs/`
- `Materials/`
- `Animations/`
- `Audio/`
- `UI/`

## Objetivo

Construir a base do jogo em Unity com uma arquitetura mais organizada, facilitando:
- desenvolvimento incremental
- versionamento no GitHub
- manutenção futura
- expansão para novos sistemas

## Tecnologias

- Unity
- C#
- Input System
- TextMesh Pro

## Observações

Este repositório ignora arquivos temporários e de cache do Unity, como:
- `Library/`
- `Temp/`
- `Logs/`
- `Obj/`
- `Build/`
- `Builds/`
- `UserSettings/`

Também foram removidos assets pesados de demonstração que não fazem parte do jogo final.

## Próximos passos

- organizar a estrutura interna de `Assets/_Project`
- criar as cenas principais
- implementar movimentação do player
- definir câmera, UI e arquitetura base dos sistemas