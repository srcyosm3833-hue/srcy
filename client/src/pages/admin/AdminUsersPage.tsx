import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { ShieldCheck, Users } from 'lucide-react'

import type { User } from '@/types'
import { useAuth } from '@/features/auth'
import { useUsers } from '@/features/user'
import { formatDate } from '@/lib/formatDate'
import { resolveAssetUrl } from '@/lib/resolveAssetUrl'
import { getInitials } from '@/lib/initials'
import { PageHeader } from '@/components/common/PageHeader'
import { EmptyState } from '@/components/common/EmptyState'
import { ErrorState } from '@/components/common/ErrorState'
import { PaginationBar } from '@/components/common/PaginationBar'
import { TableSkeleton } from '@/components/admin/TableSkeleton'
import { UserRolesDialog } from '@/components/admin/UserRolesDialog'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'

const PAGE_SIZE = 20
const COLUMNS = ['Kullanıcı', 'E-posta', 'Roller', 'Kayıt Tarihi', 'İşlemler']

/**
 * Admin kullanici yonetimi sayfasi. Listeleme Admin + Manager'a acik (A6); rol
 * atama/kaldirma yalniz Admin (backend zorlar) — "Rolleri Yönet" aksiyonu yalnizca
 * isAdmin'de gosterilir. Sayfa URL'de (?page=) tutulur. Rol yonetimi dialog'unda
 * idempotent atama + son-Admin korumasi backend kurallari toast ile ele alinir.
 */
export default function AdminUsersPage() {
  const { isAdmin } = useAuth()
  const [searchParams, setSearchParams] = useSearchParams()
  const page = Math.max(1, Number(searchParams.get('page')) || 1)

  const { data, isPending, isError, refetch, isFetching } = useUsers({
    page,
    pageSize: PAGE_SIZE,
  })

  // Rolleri yonetilen kullanici (dialog kontrollu open state'i).
  const [rolesUser, setRolesUser] = useState<User | null>(null)

  // Liste invalidate olunca parent yeniden render eder; acik dialog'daki kullaniciyi
  // guncel veriyle (yeni rollerle) eslestir ki chip'ler taze kalsin.
  const activeRolesUser = rolesUser
    ? (data?.items.find((u) => u.id === rolesUser.id) ?? rolesUser)
    : null

  function goToPage(next: number) {
    setSearchParams((prev) => {
      const params = new URLSearchParams(prev)
      params.set('page', String(next))
      return params
    })
  }

  const header = (
    <PageHeader
      title="Kullanıcı Yönetimi"
      description="Kullanıcıları görüntüleyin ve rollerini yönetin."
    />
  )

  if (isPending) {
    return (
      <div className="space-y-8">
        {header}
        <TableSkeleton columns={COLUMNS} rows={8} />
      </div>
    )
  }

  if (isError) {
    return (
      <div className="space-y-8">
        {header}
        <ErrorState
          message="Kullanıcılar yüklenemedi."
          onRetry={() => void refetch()}
        />
      </div>
    )
  }

  if (data.items.length === 0) {
    return (
      <div className="space-y-8">
        {header}
        <EmptyState
          icon={Users}
          title="Kullanıcı bulunamadı."
          description="Henüz listelenecek kullanıcı yok."
        />
      </div>
    )
  }

  return (
    <div className="space-y-8">
      {header}

      <div className="rounded-lg border border-border bg-card">
        <Table>
          <TableHeader>
            <TableRow>
              {COLUMNS.map((col) => (
                <TableHead
                  key={col}
                  className={col === 'İşlemler' ? 'text-right' : undefined}
                >
                  {col}
                </TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {data.items.map((user) => {
              const fullName =
                `${user.firstName} ${user.lastName}`.trim() || user.email
              return (
                <TableRow key={user.id}>
                  <TableCell>
                    <div className="flex items-center gap-3">
                      <Avatar className="h-8 w-8">
                        <AvatarImage
                          src={resolveAssetUrl(user.imageUrl)}
                          alt=""
                        />
                        <AvatarFallback>
                          {getInitials(fullName, user.email)}
                        </AvatarFallback>
                      </Avatar>
                      <div className="flex items-center gap-2">
                        <span className="font-medium">{fullName}</span>
                        {user.isDeleted ? (
                          <Badge variant="outline" className="text-destructive">
                            Silinmiş
                          </Badge>
                        ) : null}
                      </div>
                    </div>
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    {user.email}
                  </TableCell>
                  <TableCell>
                    {user.roles.length === 0 ? (
                      <span className="text-sm text-muted-foreground">—</span>
                    ) : (
                      <div className="flex flex-wrap gap-1">
                        {user.roles.map((role) => (
                          <Badge key={role} variant="secondary">
                            {role}
                          </Badge>
                        ))}
                      </div>
                    )}
                  </TableCell>
                  <TableCell className="whitespace-nowrap text-muted-foreground">
                    {formatDate(user.createdAt)}
                  </TableCell>
                  <TableCell className="text-right">
                    {isAdmin ? (
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => setRolesUser(user)}
                      >
                        <ShieldCheck className="h-4 w-4" />
                        Rolleri Yönet
                      </Button>
                    ) : (
                      <span className="text-xs text-muted-foreground">—</span>
                    )}
                  </TableCell>
                </TableRow>
              )
            })}
          </TableBody>
        </Table>
      </div>

      <PaginationBar
        page={data.page}
        totalPages={data.totalPages}
        hasPreviousPage={data.hasPreviousPage}
        hasNextPage={data.hasNextPage}
        onPageChange={goToPage}
        disabled={isFetching}
      />

      {/* Rol yonetimi dialog'u yalniz Admin'e acik (aksiyon da yalniz Admin'de gorunur). */}
      {isAdmin ? (
        <UserRolesDialog
          open={rolesUser !== null}
          onOpenChange={(open) => {
            if (!open) setRolesUser(null)
          }}
          user={activeRolesUser}
        />
      ) : null}
    </div>
  )
}
