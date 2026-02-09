import React, { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiGet, apiPost, apiPut, apiDelete } from '../api'

export default function VeiculosPage(){
  const qc = useQueryClient()
  const [editId, setEditId] = useState(null)
  const [filtroClienteId, setFiltroClienteId] = useState('')
  
  const clientes = useQuery({ 
    queryKey:['clientes-select'], 
    queryFn:() => apiGet('/api/clientes?pagina=1&tamanho=100') 
  })

  const veiculos = useQuery({ 
    queryKey:['veiculos', filtroClienteId], 
    queryFn:() => apiGet(`/api/veiculos${filtroClienteId ? `?clienteId=${filtroClienteId}` : ''}`) 
  })

  const [form, setForm] = useState({ placa:'', modelo:'', ano:'', clienteId:'' })

  const limparForm = () => {
    setEditId(null)
    setForm({ placa:'', modelo:'', ano:'', clienteId:'' })
  }

  const create = useMutation({
    mutationFn: (data) => apiPost('/api/veiculos', data),
    onSuccess: () => { qc.invalidateQueries({ queryKey:['veiculos'] }); limparForm(); }
  })

  const update = useMutation({
    mutationFn: (data) => apiPut(`/api/veiculos/${editId}`, data),
    onSuccess: () => { qc.invalidateQueries({ queryKey:['veiculos'] }); limparForm(); }
  })

  const handleSalvar = () => {
    const payload = { ...form, ano: form.ano ? Number(form.ano) : null }
    if (editId) update.mutate(payload)
    else create.mutate(payload)
  }

  return (
    <div>
      <h2>Veículos</h2>

      <div className="section">
        <label>Filtrar por Cliente: </label>
        <select value={filtroClienteId} onChange={e => setFiltroClienteId(e.target.value)}>
          <option value="">Todos os veículos</option>
          {clientes.data?.itens?.map(c => <option key={c.id} value={c.id}>{c.nome}</option>)}
        </select>
      </div>

      <h3>{editId ? 'Editar Veículo' : 'Novo Veículo'}</h3>
      <div className="section">
        <div className="grid grid-4">
          <input placeholder="Placa" value={form.placa} onChange={e=>setForm({...form, placa:e.target.value})}/>
          <input placeholder="Modelo" value={form.modelo} onChange={e=>setForm({...form, modelo:e.target.value})}/>
          <input placeholder="Ano" value={form.ano} onChange={e=>setForm({...form, ano:e.target.value})}/>
          
          <select value={form.clienteId} onChange={e=>setForm({...form, clienteId:e.target.value})}>
            <option value="">Selecione o Dono...</option>
            {clientes.data?.itens?.map(c => <option key={c.id} value={c.id}>{c.nome}</option>)}
          </select>

          <div style={{display:'flex', gap: 8}}>
            <button onClick={handleSalvar}>{editId ? 'Atualizar' : 'Salvar'}</button>
            {editId && <button className="btn-ghost" onClick={limparForm}>Cancelar</button>}
          </div>
        </div>
      </div>

      <h3 style={{marginTop:16}}>Lista</h3>
      <div className="section">
        {veiculos.isLoading ? <p>Carregando...</p> : (
          <table>
            <thead><tr><th>Placa</th><th>Modelo</th><th>Ano</th><th>Ações</th></tr></thead>
            <tbody>
              {veiculos.data?.map(v => (
                <tr key={v.id}>
                  <td>{v.placa}</td>
                  <td>{v.modelo}</td>
                  <td>{v.ano ?? '-'}</td>
                  <td>
                    <button className="btn-ghost" onClick={() => {
                        setEditId(v.id);
                        setForm({ placa: v.placa, modelo: v.modelo, ano: v.ano || '', clienteId: v.clienteId });
                    }}>Editar</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  )
}