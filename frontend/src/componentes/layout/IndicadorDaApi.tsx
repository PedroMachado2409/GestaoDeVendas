import { useQuery } from '@tanstack/react-query'

import { sistemaApi } from '@/api/recursos'
import { URL_API } from '@/lib/http'
import { cn } from '@/lib/utils'

/** Sonda GET /health para dizer se a indisponibilidade é da API ou da rede. */
export function IndicadorDaApi({ className }: { className?: string }) {
  const { data, isPending, isError } = useQuery({
    queryKey: ['saude-da-api'],
    queryFn: ({ signal }) => sistemaApi.saude(signal),
    refetchInterval: 30_000,
    retry: false,
  })

  const online = data?.ok === true
  const estado = isPending ? 'verificando' : isError || !online ? 'fora' : 'no ar'

  return (
    <span
      title={`${URL_API} — ${estado}`}
      className={cn(
        'inline-flex items-center gap-1.5 text-xs text-ink-muted',
        className,
      )}
    >
      <span
        aria-hidden
        className={cn(
          'size-1.5 rounded-full',
          isPending
            ? 'animar-cintilar bg-caution'
            : online
              ? 'bg-positive'
              : 'bg-negative',
        )}
      />
      <span className="hidden sm:inline">API {estado}</span>
      <span className="sr-only">API {estado}</span>
    </span>
  )
}
