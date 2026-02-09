import React, { useState } from 'react'

export default function CsvUploadPage(){
  const [log, setLog] = useState(null)

  async function handleUpload(e){
    e.preventDefault()
    const file = e.target.file.files[0]
    const fd = new FormData()
    fd.append('file', file)
    const r = await fetch((import.meta.env.VITE_API_URL || 'http://localhost:5000') + '/api/import/csv', {
      method: 'POST',
      body: fd
    })
    const j = await r.json()
    setLog(j)
  }

  return (
    <div>
      <h2>Importar Veículos via CSV</h2>
      <div className="section">
        <form onSubmit={handleUpload} style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
          <input type="file" name="file" accept=".csv" />
          <button type="submit">Processar Arquivo</button>
        </form>
      </div>

      {log && (
        <div style={{ marginTop: 20 }}>
          <h3>Relatório de Processamento</h3>
          
          {/* Resumo de Sucesso */}
          <div className="section" style={{ display: 'flex', gap: 20, background: '#f0fdf4', border: '1px solid #bbf7d0', color: '#166534' }}>
            <p><strong>Total processado:</strong> {log.processados}</p>
            <p><strong>Sucessos:</strong> {log.inseridos}</p>
            <p><strong>Falhas:</strong> {log.erros.length}</p>
          </div>

          {/* Lista de Erros Detalhada */}
          {log.erros.length > 0 && (
            <div className="section" style={{ marginTop: 10, border: '1px solid #fecaca', background: '#fef2f2' }}>
              <h4 style={{ color: '#991b1b', marginTop: 0 }}>Detalhes das Falhas:</h4>
              <ul style={{ color: '#991b1b', fontSize: '0.9rem' }}>
                {log.erros.map((erro, idx) => (
                  <li key={idx}>{erro}</li>
                ))}
              </ul>
            </div>
          )}
        </div>
      )}
      
      {!log && <p className="note">Selecione um arquivo .csv para ver o relatório.</p>}
    </div>
  )
}
