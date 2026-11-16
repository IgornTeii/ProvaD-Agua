# Prova d'Água — Web

Telas do cidadão e simulador de alagamentos, em React + TypeScript.

## Rodar sozinho

```bash
npm install
npm run dev
```

Abre em http://localhost:5173. A API precisa estar em http://localhost:5016 e o simulador em http://localhost:8100.
Para mudar os endereços, crie um arquivo `.env` com `VITE_API_URL` e `VITE_SIMULATOR_URL`.

## Telas

| Rota | Tela |
|---|---|
| `/` | Buscar meu endereço |
| `/endereco/:id` | Situação agora na minha rua |
| `/endereco/:id/comprovacoes` | Minhas comprovações |
| `/verificar/:codigo` | Verificar e imprimir uma comprovação |
| `/simular` | Simulador de alagamentos |
