# Arquipelago Perdido RPG - Unity

Projeto do jogo **Arquipélago Perdido RPG** desenvolvido em **Unity**.

## Visão geral

Este repositório contém a nova versão do projeto em Unity, organizada para facilitar a evolução do jogo com uma estrutura mais limpa, versionamento correto e separação entre assets do projeto e pacotes externos.

## Estrutura principal

- `Assets/` → cenas, scripts, prefabs, materiais, UI e pacotes usados no projeto
- `Packages/` → dependências do Unity
- `ProjectSettings/` → configurações do projeto
- `.gitignore` → exclusões de cache, builds e arquivos locais
- `README.md` → documentação inicial do projeto

## Organização em Assets

A pasta principal de desenvolvimento é:

- `Assets/_Project/`

Estrutura sugerida:

- `Assets/_Project/Scenes`
- `Assets/_Project/Scripts`
- `Assets/_Project/Prefabs`
- `Assets/_Project/Materials`
- `Assets/_Project/Animations`
- `Assets/_Project/Audio`
- `Assets/_Project/UI`
- `Assets/_Project/Art`
- `Assets/_Project/Resources`
- `Assets/_Project/Editor`

## Tecnologias

- Unity
- C#
- Input System
- TextMesh Pro

## Objetivo

Construir a base do jogo em Unity com uma arquitetura organizada, facilitando:
- desenvolvimento incremental
- manutenção futura
- versionamento no GitHub
- expansão para novos sistemas e mecânicas

## Observações

O repositório ignora arquivos temporários e de cache do Unity, como:
- `Library/`
- `Temp/`
- `Logs/`
- `Obj/`
- `Build/`
- `Builds/`
- `UserSettings/`

Também foram removidos assets pesados de demonstração que não fazem parte do jogo final.

## Próximos passos

- organizar as cenas principais
- definir a estrutura base de scripts
- implementar movimentação do player
- configurar câmera, UI e fluxo inicial do jogo